using System;
using System.Xml.Serialization;
using Fusion.Engine.Common;

namespace FusionUI
{
    public abstract class AbstractTimeManager {
        private Game Game;

        public enum TimeState {
            PlayForward,
            PlayBackward,
            Stop
        }

		public string Name = "";

        public TimeState State { get; protected set; } = TimeState.Stop;

        public DateTime CurrentTime;
        public TimeSpan TimeStep = TimeSpan.FromMinutes(60);

        public DateTime StartTime = DateTime.Now.AddDays(-1);
        public DateTime EndTime = DateTime.Now;

        public bool IsAutoRewind { set; get; } = true;

        private bool resetTimes = false;
		[XmlIgnore]
		public Func<string> TimeFunc = null, DateFunc = null;

        public bool UpdateLayers = true;

        public bool UpdateOnDrag = false;

        public AbstractTimeManager (Game game) {
            Game = game;
        }


        public abstract void ResetTimes();


        public abstract void Update(GameTime gameTime);


        public void PlayForward () {
            State = TimeState.PlayForward;
        }

        public void PlayBackward () {
            State = TimeState.PlayBackward;
        }

        public void Stop () {
            State = TimeState.Stop;
        }


        public void StepForward () {
            if (TimeStep < DateTime.MaxValue - CurrentTime) {
                CurrentTime = CurrentTime.Add (TimeStep);
            } else {
                CurrentTime = DateTime.MaxValue;
            }
        }

        public void StepBackward () {
            if (TimeStep < (CurrentTime - DateTime.MinValue)) {
                CurrentTime = CurrentTime.Subtract (TimeStep);
            } else {
                CurrentTime = DateTime.MinValue;
            }
        }

    }
}
