namespace gitter.Framework.Controls
{
	using System;
	using System.Drawing;

	using Resources = gitter.Framework.Properties.Resources;

	public sealed class ViewButton
	{
		#region Static Data

		private static readonly Bitmap ImgMenu = Resources.ImgViewHostMenu;
		private static readonly Bitmap ImgNormalize = Resources.ImgNormalize;
		private static readonly Bitmap ImgMaximize = Resources.ImgMaximize;
		private static readonly Bitmap ImgPin = Resources.ImgViewHostPin;
		private static readonly Bitmap ImgClose = Resources.ImgViewHostClose;
		private static readonly Bitmap ImgScrollLeft = Resources.ImgTabScrollLeft;
		private static readonly Bitmap ImgScrollRight = Resources.ImgTabScrollRight;
		private static readonly Bitmap ImgTabMenu = Resources.ImgTabMenu;
		private static readonly Bitmap ImgTabMenuExt = Resources.ImgTabMenuExtends;

		#endregion

		#region Data

		private int _offset;
		private ViewButtonType _type;
		private Image _image;

		#endregion

		internal ViewButton(int offset, ViewButtonType type)
		{
			_offset = offset;
			_type = type;
			switch(_type)
			{
				case ViewButtonType.Menu:
					_image = ImgMenu;
					break;
				case ViewButtonType.Pin:
					_image = ImgPin;
					break;
				case ViewButtonType.Unpin:
					_image = ImgPin;
					break;
				case ViewButtonType.Normalize:
					_image = ImgNormalize;
					break;
				case ViewButtonType.Maximize:
					_image = ImgMaximize;
					break;
				case ViewButtonType.Close:
					_image = ImgClose;
					break;
				case ViewButtonType.ScrollTabsLeft:
					_image = ImgScrollLeft;
					break;
				case ViewButtonType.ScrollTabsRight:
					_image = ImgScrollRight;
					break;
				case ViewButtonType.TabsMenu:
					_image = ImgTabMenu;
					break;
				case ViewButtonType.TabsScrollMenu:
					_image = ImgTabMenuExt;
					break;
				default:
					_image = null;
					break;
			}
		}

		#region Properties

		public int Offset
		{
			get { return _offset; }
		}

		public ViewButtonType Type
		{
			get { return _type; }
		}

		public Image Image
		{
			get { return _image; }
		}

		#endregion

		internal void OnPaint(Graphics graphics, Rectangle bounds, bool focus, bool hover, bool pressed)
		{
			ViewManager.Renderer.RenderViewButton(this, graphics, bounds, focus, hover, pressed);
		}

		public override string ToString()
		{
			return _type.ToString();
		}
	}
}
