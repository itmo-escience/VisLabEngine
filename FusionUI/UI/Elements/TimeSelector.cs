using System;
using System.Collections.Generic;
using System.Globalization;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements
{
    public class TimeSelector : ScalableFrame
    {
        private DateTime defaultVal;
        public DateTime Current
        {
            get
            {
                DateTime t = new DateTime();
                if (DateTime.TryParse(
                    $"{DayHolder.Current}/{MonthHolder.Current}/{YearHolder.Current} {TimeHolder.Current}", out t))
                    return t;
                return defaultVal;
            }
            set
            {
                YearHolder.Current = $"{value.Year:0000}";
                MonthHolder.Current = value.ToString("MMM");
                DayHolder.Current = $"{value.Day:00}";
                TimeHolder.Current = value.ToString("HH:mm");
                UpdateLabel();
            }
        }

        public DateTime Last;
        public Action<DateTime> UpdateValue;
        public Action<string> UpdateYear, UpdateMonth, UpdateDay, UpdateTime;

        private bool isOpen = false;

        private bool isClick = false;

        public float Capacity = 6;
        public int yearOffset = 0, monthOffset = 0, dayOffset = 0, timeOffset = 0;

        public DateTime StartTime
        {
            get { return startTime; }
            set
            {
                startTime = value;
                if (endTime < startTime) endTime = startTime;
                UpdateDates(Current);
            }
        }

        public DateTime EndTime
        {
            get { return endTime; }
            set
            {
                endTime = value;
                if (startTime > endTime) startTime = endTime;
                UpdateDates(Current);
            }
        }

        public void SetDates(DateTime start, DateTime end, DateTime? current = null)
        {
            startTime = start;
            endTime = end;
            var initialDate = current != null ? current.Value : Current;
            if (initialDate < start) initialDate = start;
            if (initialDate > end) initialDate = end;
            defaultVal = initialDate;
            UpdateDates(initialDate);
            Current = current.Value;
        }

        private DateTime startTime = DateTime.MinValue, endTime = DateTime.MaxValue;

        public void SetValue(DateTime date)
        {
            YearHolder.Current = date.ToString("yyy");
            MonthHolder.Current = date.ToString("MMM");
            DayHolder.Current = date.ToString("dd");
            TimeHolder.Current= date.ToString("HH:mm");            
        }

        public TimeSelector(FrameProcessor ui, float x, float y, float w, float h, Color backColor, Action<DateTime> selectAction, Color borderColor, DateTime initialDate, DateTime minDate, DateTime maxDate) : base(ui, x, y, w, h, "", backColor)
        {
            BorderColor = borderColor;            

            init(ui);
            UpdateValue += selectAction;

            startTime = minDate;
            endTime = maxDate;
            if (initialDate < minDate) initialDate = minDate;
            if (initialDate > maxDate) initialDate = maxDate;
            defaultVal = initialDate;
            UpdateDates(initialDate);
            //TODO: parse this from date passed to constructor
            SetValue(initialDate);
            UpdateLabel();
        }

        void UpdateLabel()
        {
            YearRow.Text = YearHolder.Current;
            MonthRow.Text = MonthHolder.Current;
            DayRow.Text = DayHolder.Current;
            TimeRow.Text = TimeHolder.Current;
        }

        public ScalableFrame MainHolder, YearRow,  MonthRow,  DayRow,  TimeRow;
        public TimeSelectorScroll YearHolder, MonthHolder, DayHolder, TimeHolder;

        void init(FrameProcessor ui)
        {            
            YearRow = new ScalableFrame(ui, 0, 0, UnitWidth / 4, UnitHeight, "", Color.Zero)
            {
                Border = 2,
                BorderColor = BorderColor,  
                TextAlignment              =  Alignment.MiddleCenter,
            };
            this.Add(YearRow);
            MonthRow = new ScalableFrame(ui, UnitWidth / 4, 0, UnitWidth / 4, UnitHeight, "", Color.Zero)
            {
                Border = 2,
                BorderColor = BorderColor,
                TextAlignment = Alignment.MiddleCenter,
            };
            this.Add(MonthRow);
            DayRow = new ScalableFrame(ui, UnitWidth / 4 * 2, 0, UnitWidth / 4, UnitHeight, "", Color.Zero)
            {
                Border = 2,
                BorderColor = BorderColor,
                TextAlignment = Alignment.MiddleCenter,
            };
            this.Add(DayRow);
            TimeRow = new ScalableFrame(ui, UnitWidth / 4 * 3, 0, UnitWidth / 4, UnitHeight, "", Color.Zero)
            {
                Border = 2,
                BorderColor = BorderColor,
                TextAlignment = Alignment.MiddleCenter,
            };
            this.Add(TimeRow);

            MainHolder = new ScalableFrame(ui, 0, -UIConfig.UnitSelectorRowHeight * Capacity / 2, UnitWidth, UIConfig.UnitSelectorRowHeight * (Capacity + 1), "", UIConfig.PopupColor)
            {
                Border = 2,
                BorderColor = BorderColor,
                Visible = false,
                SuppressActions = true,
            };
            MainHolder.ZOrder = 1000;
            ApplicationInterface.Instance.rootFrame.Add(MainHolder);

            var ConfirmButton = new Button(ui, 0, MainHolder.UnitHeight - UIConfig.UnitSelectorRowHeight, UnitWidth/2,
                UIConfig.UnitSelectorRowHeight, "OK", UIConfig.PopupColor, UIConfig.ActiveColor, 200, () => closeList(true))
            {                
                BorderColor =  BorderColor,
                TextAlignment = Alignment.MiddleCenter,                
            };
            MainHolder.Add(ConfirmButton);
            var CancelButton = new Button(ui, UnitWidth / 2, MainHolder.UnitHeight - UIConfig.UnitSelectorRowHeight, UnitWidth / 2,
                UIConfig.UnitSelectorRowHeight, "Cancel", UIConfig.PopupColor, UIConfig.ActiveColor, 200, () => closeList())
            {                
                BorderColor = BorderColor,
                TextAlignment = Alignment.MiddleCenter,
            };
            MainHolder.Add(CancelButton);

            this.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
                if (!args.IsClick) return;
                openList();                                        
            };

            MainHolder.ActionOut += (ControlActionArgs args, ref bool flag) =>
            {                
                if (!MainHolder.Selected) closeList();                
            };
            #region Years
            YearHolder = new TimeSelectorScroll(ui, 0, 0, UnitWidth/4,
                UIConfig.UnitSelectorRowHeight*(Capacity), "", UIConfig.PopupColor)
            {
                Border = 2,
                BorderColor = BorderColor,
                Capacity = Capacity,
                UpdateFunction = (s) => UpdateDates(Current),
            };
            MainHolder.Add(YearHolder);       
            #endregion
            #region Months          
            MonthHolder = new TimeSelectorScroll(ui, UnitWidth / 4, 0, UnitWidth / 4,
                UIConfig.UnitSelectorRowHeight * (Capacity), "", UIConfig.PopupColor)
            {
                Border = 2,
                BorderColor = BorderColor,
                Capacity = Capacity,
                UpdateFunction = (s) =>
                {
                    var cur = Current;
                    var n = DateTime.DaysInMonth(cur.Year, DateTime.ParseExact(s, "MMM", CultureInfo.CurrentCulture).Month);
                    var d = Math.Min(int.Parse(DayHolder.Current), n);
                    var newD = DateTime.Parse($"{d:00}/{s}/{YearHolder.Current} {TimeHolder.Current}");
                    UpdateDates(newD);
                    Current = newD;
                },
            };
            MainHolder.Add(MonthHolder);
            #endregion
            #region Days
            DayHolder = new TimeSelectorScroll (ui, UnitWidth / 4 * 2, 0, UnitWidth / 4,
                UIConfig.UnitSelectorRowHeight * (Capacity), "", UIConfig.PopupColor)
            {
                Border = 2,
                BorderColor = BorderColor,
                Capacity = Capacity,
                UpdateFunction = (s) => UpdateDates(Current),
            };
            MainHolder.Add(DayHolder);
            #endregion
            #region Times
            TimeHolder = new TimeSelectorScroll (ui, UnitWidth / 4 * 3, 0, UnitWidth / 4,
                UIConfig.UnitSelectorRowHeight * (Capacity), "", UIConfig.PopupColor)
            {
                Border = 2,
                BorderColor = BorderColor,
                Capacity = Capacity,
                UpdateFunction = (s) => UpdateDates(Current),
            };
            MainHolder.Add(TimeHolder);
            #endregion
            
        }

        protected override void Update(GameTime gameTime)
        {
            MainHolder.ZOrder = 1000;
        }

        public void UpdateDates(DateTime cur)
        {
            int curYear = cur.Year, curMonth = cur.Month, curDay = cur.Day;
            curYear = MathUtil.Clamp(cur.Year, StartTime.Year, EndTime.Year);
            curMonth = curYear == StartTime.Year
                ? Math.Max(cur.Month, StartTime.Month)
                : curYear == EndTime.Year
                    ? Math.Min(cur.Month, EndTime.Month) : cur.Month;
            curDay = curYear == StartTime.Year && cur.Month == StartTime.Month
                ? Math.Max(cur.Day, StartTime.Day)
                : cur.Month == EndTime.Month
                    ? Math.Min(cur.Day, EndTime.Day)
                    : cur.Day;
            curDay = Math.Min(curDay, DateTime.DaysInMonth(curYear, curMonth));

            var YearList = new List<string>();
            
            for (int i = StartTime.Year; i <= EndTime.Year; i++)
            {
                YearList.Add($"{i:0000}");
            }
            SetYears(YearList);
            UpdateYear?.Invoke(YearHolder.Current);

            var MonthList = new List<string>();            
            for (var i = new DateTime(curYear, curYear == startTime.Year ? startTime.Month : 1, 1); i.Year == curYear && i <= EndTime; i = i.AddMonths(1))
            {
                MonthList.Add(i.ToString("MMM"));
            }
            SetMonths(MonthList);            
            UpdateMonth?.Invoke(MonthHolder.Current);

            //cur = Current;
            var DaysList = new List<string>();
            for (var i = new DateTime(curYear, curMonth, curYear == startTime.Year && curMonth == startTime.Month ? startTime.Day : 1); i.Month == curMonth && i <= EndTime; i = i.AddDays(1))
            {
                DaysList.Add(i.ToString("dd"));
            }
            SetDays(DaysList);
            UpdateDay?.Invoke(DayHolder.Current);

            //cur = Current;
            var TimesList = new List<string>();
            if (cur < StartTime) cur = StartTime;
            if (cur > EndTime) cur = EndTime;
            DateTime ts = curYear == StartTime.Day && curMonth == StartTime.Month && curDay == StartTime.Day
                ? new DateTime(curYear, curMonth, curDay, StartTime.Hour, StartTime.Minute, 0)
                : new DateTime(curYear, curMonth, curDay);            
            DateTime et = curYear == EndTime.Day && curMonth == EndTime.Month && curDay == EndTime.Day 
                ? new DateTime(curYear, curMonth, curDay, EndTime.Hour, EndTime.Minute, 0)
                : new DateTime(curYear, curMonth, curDay).AddDays(1);
            for (; ts < et; ts = ts.AddMinutes(30))
            {
                TimesList.Add(ts.ToString("HH:mm"));
            }
            SetTimes(TimesList);
            UpdateTime?.Invoke(TimeHolder.Current);           
        }

        public void SetYears(List<string> yearsList )
        {            
            YearHolder.SetRows(yearsList);
            UpdateLabel();
        }

        public void SetMonths (List<string> monthList) 
        {
            MonthHolder.SetRows(monthList);
            UpdateLabel ();
        }

        public void SetDays(List<string> daysList)
        {
            DayHolder.SetRows(daysList);
            UpdateLabel ();
        }

        public void SetTimes(List<string> timesList)
        {
            TimeHolder.SetRows(timesList);
            UpdateLabel ();
        }      

        void openList()
        {
            if (isOpen) return;
            isOpen = true;
            MainHolder.Selected = true;
            MainHolder.X = this.GlobalRectangle.X - ApplicationInterface.Instance.rootFrame.GlobalRectangle.X;
            MainHolder.Y = this.GlobalRectangle.Y - ApplicationInterface.Instance.rootFrame.GlobalRectangle.Y;
            MainHolder.UnitY -= (Capacity-1)/2*UIConfig.UnitSelectorRowHeight + this.Y/2;
            MainHolder.Visible = true;
            Last = Current;
            MainHolder.ZOrder = 1000;
        }

        void closeList(bool confirm = false)
        {      
            if (!isOpen) return;
            isOpen = false;
            
            MainHolder.Visible = false;

            if (!confirm) SetValue(Last);
            UpdateLabel();
            UpdateValue(Current);
        }
    }
}
