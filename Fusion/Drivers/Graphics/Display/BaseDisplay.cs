﻿using System;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using System.Windows.Forms;
using Forms = System.Windows.Forms;
using Fusion.Engine.Common;
using Fusion.Core.Mathematics;
using Fusion.Input.Touch;


namespace Fusion.Drivers.Graphics.Display
{
	public abstract class BaseDisplay : GraphicsResource
	{
		protected readonly	Game Game;
		internal D3D.Device d3dDevice = null;

		protected Ubershader	stereo;
		protected StateFactory	factory;

		protected enum Flags {
			VERTICAL_LR		=	0x0001,
			VERTICAL_RL		=	0x0002,
			HORIZONTAL_LR	=	0x0004,
			HORIZONTAL_RL	=	0x0008,
			OCULUS_RIFT		=	0x0010,
		}



		/// <summary>
		///
		/// </summary>
		/// <param name="parameters"></param>
		internal BaseDisplay( Game game, GraphicsDevice device, GraphicsParameters parameters ) : base(device)
		{
			this.Game	=	game;

			ShowAdapterInfo( parameters );
		}



		/// <summary>
		///
		/// </summary>
		internal virtual void CreateDisplayResources ()
		{
			Game.Reloading += (s,e) => LoadContent();
			LoadContent();
		}



		/// <summary>
		///
		/// </summary>
		private void LoadContent ()
		{
			stereo	=	Game.Content.Load<Ubershader>("stereo");
			factory	=	stereo.CreateFactory( typeof(Flags), Primitive.TriangleList, VertexInputElement.Empty, BlendState.Opaque, RasterizerState.CullNone, DepthStencilState.None );
		}


		internal virtual void Resize(int newWidth, int newHeight)
		{

		}


		/// <summary>
		///
		/// </summary>
		/// <param name="left">Left source buffer</param>
		/// <param name="right">Right source buffer</param>
		/// <param name="leftResolved">Buffer to resolve left MSAA buffer. (NULL if left buffer is not MSAA buffer)</param>
		/// <param name="rightResolved">Buffer to resolve right MSAA buffer. (NULL if right buffer is not MSAA buffer)</param>
		/// <param name="destination">Target buffer</param>
		/// <param name="mode">Ubershader flag</param>
		protected void MergeStereoBuffers ( RenderTarget2D left, RenderTarget2D right, RenderTarget2D leftResolved, RenderTarget2D rightResolved, RenderTarget2D destination, Flags flag )
		{
			device.ResetStates();

			device.SetTargets( null, destination );

			if (leftResolved!=null) {
				device.Resolve( left, leftResolved );
			}
			if (rightResolved!=null) {
				device.Resolve( right, rightResolved );
			}


			device.PipelineState		=	factory[ (int)flag ];

			device.PixelShaderSamplers[0]	=	SamplerState.LinearClamp;
			device.PixelShaderResources[0]	=	leftResolved  == null ? left  : leftResolved;
			device.PixelShaderResources[1]	=	rightResolved == null ? right : rightResolved;

			device.SetupVertexInput( null, null, null );
			device.Draw( 3, 0 );
		}



		/// <summary>
		///
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref d3dDevice );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Current stereo eye
		/// </summary>
		public abstract StereoEye TargetEye {
			get; internal set;
		}

		public abstract bool Focused {
			get;
		}

		/// <summary>
		/// List of stereo eye to render.
		/// </summary>
		public abstract StereoEye[] StereoEyeList {
			get;
		}



		/// <summary>
		/// Get backbuffer
		/// </summary>
		public abstract RenderTarget2D	BackbufferColor {
			get;
		}



		/// <summary>
		/// Gets default depth buffer
		/// </summary>
		public abstract DepthStencil2D	BackbufferDepth {
			get;
		}



		/// <summary>
		/// Sets and gets fullscreen mode
		/// </summary>
		public abstract bool Fullscreen {
			get;
			internal set;
		}



		/// <summary>
		/// Gets display bounds.
		/// </summary>
		public abstract Rectangle Bounds {
			get;
		}



		/// <summary>
		///
		/// </summary>
		public abstract Form Window {
			get;
		}



		/// <summary>
		///
		/// </summary>
		public abstract void Prepare ();



		/// <summary>
		///
		/// </summary>
		/// <param name="syncInterval"></param>
		public abstract void SwapBuffers ( int syncInterval );



		/// <summary>
		///
		/// </summary>
		public abstract void Update ();


		/// <summary>
		///
		/// </summary>
		/// <param name="window"></param>
		/// <param name="fullscr"></param>
		protected delegate void ChangeFullscreenDelegate(Form window, bool fullscr);
		protected ChangeFullscreenDelegate changeFullscreen = new ChangeFullscreenDelegate( ChangeFullscreen );

		/// <summary>
		///
		/// </summary>
		/// <param name="window"></param>
		/// <param name="fullscr"></param>
		private static void ChangeFullscreen ( Form window, bool fullscr )
		{
			if (fullscr) {
				window.FormBorderStyle	=	FormBorderStyle.None;
				window.WindowState		=	FormWindowState.Maximized;
				//window.TopMost			=	false;
			} else {
				window.FormBorderStyle	=	FormBorderStyle.Sizable;
				window.WindowState		=	FormWindowState.Normal;
				window.TopMost			=	false;
			}
		}



		/// <summary>
		///
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal Form CreateForm ( GraphicsParameters parameters, Output output )
		{
			var form = new Form() {
				Text			=	Game.GameTitle,
				BackColor		=	System.Drawing.Color.Black,
				ClientSize		=	new System.Drawing.Size( parameters.Width, parameters.Height ),
				Icon			=	Game.Icon ?? Fusion.Properties.Resources.fusionIcon,
				//ControlBox		=	false,
				StartPosition	=	output==null ? FormStartPosition.CenterScreen : FormStartPosition.Manual,
			};


			if (output!=null) {

				var bounds		=	output.Description.DesktopBounds;
				var scrW		=	bounds.Right - bounds.Left;
				var scrH		=	bounds.Bottom - bounds.Top;

				form.Location	=	new System.Drawing.Point( bounds.Left + (scrW - form.Width)/2, bounds.Top + (scrH - form.Height)/2 );
				form.Text		+=	" - [" + output.Description.DeviceName + "]";
			}

			form.KeyDown += form_KeyDown;
			form.KeyUp += form_KeyUp;
			form.KeyPress += form_KeyPress;
			form.Resize += (s,e) => Game.InputDevice.RemoveAllPressedKeys();
			form.Move += (s,e) => Game.InputDevice.RemoveAllPressedKeys();
			form.FormClosing += form_FormClosing;

			ChangeFullscreen( form, parameters.FullScreen );

			return form;
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		internal Form CreateTouchForm(GraphicsParameters parameters, Output output)
		{
			var form = new TouchForm() {
				Text			=	Game.GameTitle,
				BackColor		=	System.Drawing.Color.Black,
				ClientSize		=	new System.Drawing.Size(parameters.Width, parameters.Height),
				Icon			=	Game.Icon ?? Fusion.Properties.Resources.fusionIcon,
				//ControlBox		=	false,
				StartPosition	=	output == null ? FormStartPosition.CenterScreen : FormStartPosition.Manual,
			};


			if (output != null) {
				var bounds	= output.Description.DesktopBounds;
				var scrW	= bounds.Right - bounds.Left;
				var scrH	= bounds.Bottom - bounds.Top;

				form.Location = new System.Drawing.Point(bounds.Left + (scrW - form.Width) / 2, bounds.Top + (scrH - form.Height) / 2);
				form.Text += " - [" + output.Description.DeviceName + "]";
			}

			form.KeyDown	+= form_KeyDown;
			form.KeyUp		+= form_KeyUp;
			form.KeyPress	+= form_KeyPress;
			form.Resize		+= (s, e) => Game.InputDevice.RemoveAllPressedKeys();
			form.Move		+= (s, e) => Game.InputDevice.RemoveAllPressedKeys();

			form.TouchTap			+= (args) => Game.InputDevice.NotifyTouchTap(args);
			form.TouchDoubleTap		+= (args) => Game.InputDevice.NotifyTouchDoubleTap(args);
			form.TouchSecondaryTap	+= (args) => Game.InputDevice.NotifyTouchSecondaryTap(args);
			form.TouchManipulation	+= (args) => Game.InputDevice.NotifyTouchManipulation(args);
            form.TouchHold          += (args) => Game.InputDevice.NotifyTouchHold(args);
			form.FormClosing += form_FormClosing;

			ChangeFullscreen(form, parameters.FullScreen);


			//////////////////////
			//SharpDX.DirectManipulation.Manager m = new Manager();
			//m.Activate(form.Handle);
			//
			//IntPtr updateManagerPtr;
			//m.GetUpdateManager(new Guid("B0AE62FD-BE34-46E7-9CAA-D361FACBB9CC"), out updateManagerPtr);
			//
			//var updateManager = new UpdateManager(updateManagerPtr);
			//
			//var compositor = Compositor2.CreateDefaultDirectCompositor();
			//
			//compositor.SetUpdateManager(updateManager);
			//
			//var frame = compositor.QueryInterfaceOrNull<FrameInfoProvider>();

			return form;
		}


		private void form_FormClosing ( object sender, FormClosingEventArgs e )
		{
			if (Game.ExitRequested) {
				e.Cancel	=	false;
			} else {
				Game.GameInterface.RequestToExit();
				e.Cancel	=	true;
			}
		}



		private void form_KeyPress ( object sender, KeyPressEventArgs e )
		{
			Game.InputDevice.NotifyKeyPress( e.KeyChar );
		}



		private void form_KeyUp ( object sender, KeyEventArgs e )
		{
			Game.InputDevice.NotifyKeyUp( (Fusion.Drivers.Input.Keys)(int)e.KeyCode, e.Alt, e.Shift, e.Control );
		}



		private void form_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Alt && e.KeyCode==Forms.Keys.Enter) {
				Fullscreen = !Fullscreen;
			}

			Game.InputDevice.NotifyKeyDown( (Fusion.Drivers.Input.Keys)(int)e.KeyCode, e.Alt, e.Shift, e.Control );
		}



		/// <summary>
		///
		/// </summary>
		protected void ShowAdapterInfo ( GraphicsParameters parameters )
		{
			Log.Message("Mode : {0}x{1} {3} MS:{2} Stereo:{5} {4}",
				parameters.Width,
				parameters.Height,
				0,
				parameters.FullScreen ? "FS" : "W",
				parameters.UseDebugDevice ? "(Debug)" : "",
				parameters.StereoMode );

			using ( var factory2 = new Factory1() ) {

				Log.Message("Adapters:");

				try {
					foreach (var adapter in factory2.Adapters) {
						var aDesc = adapter.Description;
						Log.Message("   {0} - {1}", aDesc.Description, D3D.Device.GetSupportedFeatureLevel(adapter));

						foreach ( var output in adapter.Outputs ) {
							var desc = output.Description;
							var bnds = output.Description.DesktopBounds;
							var bndsString = string.Format("x:{0} y:{1} w:{2} h:{3}", bnds.Left, bnds.Top, bnds.Right-bnds.Left, bnds.Bottom-bnds.Top );

							Log.Message("   {0} [{1}] {2}", desc.DeviceName, bndsString, desc.Rotation );
						}
					}
				} catch ( Exception e ) {
					Log.Warning( e.Message );
				}
			}
		}
	}
}
