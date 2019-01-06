using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Styling;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WalletWasabi.Helpers;
using FontFamily = Avalonia.Media.FontFamily;

namespace WalletWasabi.Gui.Controls
{
	public class NoparaPasswordBox : ExtendedTextBox, IStyleable
	{
		Type IStyleable.StyleKey => typeof(TextBox);

		public static readonly string[] Titles =
		{
			"这个笨老外不知道自己在写什么。", // This silly foreigner does not know what he is writing.
			"法式炸薯条法式炸薯条法式炸薯条", //French fries, french fries, french fries
			"只有一支筷子的人会挨饿。", //Man with one chopstick go hungry.
			"说太多灯泡笑话的人，很快就会心力交瘁。", // Man who tell one too many light bulb jokes soon burn out!
			"汤面火锅", //Noodle soup, hot pot
			"你是我见过的最可爱的僵尸。", //You’re the cutest zombie I’ve ever seen.
			"永不放弃。", //Never don't give up.
			"如果你是只宠物小精灵，我就选你。" //If you were a Pokemon, I'd choose you.
			"你的比特币你的钥匙。" //Your keys, your bicoin.
			"用户激活软叉。" //User Activated Soft Fork.
			"没有两个x。" //No2X.
			"不信任,验证。" //Don't trust, verify.
			"在银行的第二轮救助边缘的财政大臣。" //Chancellor on brink of second baylout for banks.
			"比特币：一种点对点的电子现金系统。" //Bitcoin: a electronic peer to peer cash system.
			"个纯粹的点对点版本的电子现金系统，将允许在线支付直接从一方发送到另一方，而无需通过金融机构。" //A purely peer-to-peer version of electronic cash would allow online payments to be sent directly from one party to another without going through a financial institution.
			"我开发了一个名为比特币的新型开源P2P电子现金系统。" //I've developed a new open source P2P e-cash system called Bitcoin.
			"隐私是电子时代开放社会的必要条件。" //Privacy is necessary for an open society in the electronic age.
			"隐私不是保密。" //Privacy is not secrecy.
			"一个幽灵正在困扰着现代世界，即加密无政府状态的幽灵。" //A specter is haunting the modern world, the specter of crypto anarchy. 
			"我们正在建立一个私人互动中心，统治者不能干涉。" //We are building a center of private interaction, where rulers cannot intrude.
			"最基本的文化武器是控制言论。" //The most fundamental weapon of culture is the control of speech.
			"独处是最值得珍惜的事情。" //To be left alone is the most precious thing one can ask of.
			"在抵制不公正方面不存在犯罪意图。" //There can be no criminal intent in resisting injustice.
			"可以通过使用特殊信封获得解决方案。" //A solution can be obtained by use of the special envelopes.
			"你们中间不受欢迎。 你们聚集的地方没有主权。" //You are not welcome among us. You have no sovereignty where we gather. 
			"制作密码和代码的科学。" //The science of the making of ciphers and codes.
			"不透明意味着没有透明，不可见。" //Opaque means not transparent, not visible.
			"没有中央控制，没有统治者，没有领导者。" //No central control, no ruler, no leader.
			"匿名系统使个人能够在需要时展示自己的身份，并且只有在需要时才能展示自己的身份。" //Anoanonymous system empowers individuals to reveal their identity when desired and only when desired.
			"绝对隐私和匿名交易将成为一个公民社会。" //Absolute privacy and anonymous transactions would make for a civil society.
			"权力在我心中。 它一直都是。" //The power is within us. It always has been.
		};

		//public static readonly string[] Titles =
		//{
		//	"apple",
		//	"banana",
		//	"grape",
		//	"orange",
		//	"blackberry",
		//};

		private static readonly Key[] SuppressedKeys =
			{ Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift, Key.Escape, Key.CapsLock, Key.NumLock, Key.LWin, Key.RWin };

		private bool _supressChanges;
		private string _displayText = "";
		private Random _random = new Random();
		private StringBuilder _sb = new StringBuilder();
		private HashSet<Key> _supressedKeys;

		public int SelectionLength => SelectionEnd - SelectionStart;

		public static readonly StyledProperty<string> PasswordProperty =
			AvaloniaProperty.Register<NoparaPasswordBox, string>(nameof(Password), defaultBindingMode: BindingMode.TwoWay);

		public string Password
		{
			get => GetValue(PasswordProperty);
			set
			{
				SetValue(PasswordProperty, value);
			}
		}

		public static readonly StyledProperty<string> FixedPasswordTextProperty =
			AvaloniaProperty.Register<NoparaPasswordBox, string>(nameof(FixedPasswordText), defaultBindingMode: BindingMode.TwoWay);

		public string FixedPasswordText
		{
			get => GetValue(FixedPasswordTextProperty);
			set
			{
				SetValue(FixedPasswordTextProperty, value);
			}
		}

		protected override bool IsCopyEnabled => false;

		public NoparaPasswordBox()
		{
			this.GetObservable(PasswordProperty).Subscribe(x =>
			{
				Password = x;
				if (string.IsNullOrEmpty(x)) //clean the passwordbox
				{
					_sb.Clear();
					OnTextInput(x);
				}
			});

			this.GetObservable(FixedPasswordTextProperty).Subscribe(x =>
			{
				FixedPasswordText = x;
			});

			string fontName = "SimSun"; //https://docs.microsoft.com/en-us/typography/font-list/simsun
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				fontName = "PingFang TC"; //https://en.wikipedia.org/wiki/List_of_typefaces_included_with_macOS
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				fontName = "Noto Sans CJK TC"; //https://www.pinyinjoe.com/linux/ubuntu-10-chinese-fonts-openoffice-language-features.htm
											   //The best and simplest way is to use console command (this command should be available for all ubuntu-based distributions)
											   //fc - list
			}

			try
			{
				var fontTester = SKTypeface.FromFamilyName(fontName);
				if (fontTester.FamilyName == fontName)
				{
					FontFamily = FontFamily.Parse(fontName); //use the font
				}
				else
				{
					throw new FormatException("font is missing fallback to default font");
				}
			}
			catch (Exception)
			{
				PasswordChar = '*'; //use passwordchar instead
			}
			_supressedKeys = new HashSet<Key>(SuppressedKeys);
		}

		private void GenerateNewRandomSequence()
		{
			StringBuilder sb = new StringBuilder();
			do
			{
				List<string> ls = new List<string>();
				if (string.IsNullOrEmpty(FixedPasswordText))
				{
					ls.AddRange(Titles);
				}
				else ls.Add(FixedPasswordText);

				do
				{
					var s = ls[_random.Next(0, ls.Count - 1)];
					sb.Append(s);
					ls.Remove(s);
					if (sb.Length >= Constants.MaxPasswordLength) break;
				}
				while (ls.Count > 0);
			}
			while (sb.Length < Constants.MaxPasswordLength); //generate more text using the same sentences
			_displayText = sb.ToString();
		}

		protected override async void OnKeyDown(KeyEventArgs e)
		{
			if (_supressedKeys.Contains(e.Key))
			{
				return;
			}
			//prevent copy
			if ((e.Key == Key.C || e.Key == Key.Insert) && (e.Modifiers == InputModifiers.Control || e.Modifiers == InputModifiers.Windows))
			{
				return;
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				if (e.Key == Key.V && e.Modifiers == InputModifiers.Control) //prevent paste
				{
					return;
				}
			}

			bool paste = false;
			if (e.Key == Key.V)
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					if (e.Modifiers == InputModifiers.Windows) paste = true;
				}
				else
				{
					if (e.Modifiers == InputModifiers.Control) paste = true;
				}
			}

			if (paste) //paste
			{
				var clipboard = (IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard));
				Task<string> clipboardTask = clipboard.GetTextAsync();
				string text = await clipboardTask;
				if (!string.IsNullOrEmpty(text))
				{
					e.Handled = OnTextInput(text);
				}
			}
			else if (e.Key == Key.Back && _sb.Length > 0) //backspace button -> delete from the end
			{
				if (SelectionLength != 0)
				{
					_sb.Clear();
				}
				else
				{
					if (CaretIndex == Text.Length)
					{
						_sb.Remove(_sb.Length - 1, 1);
					}
				}
				e.Handled = true;
			}
			else if (e.Key == Key.Delete && _sb.Length > 0) //delete button -> delete from the beginning
			{
				if (SelectionLength != 0)
				{
					_sb.Clear();
				}
				else
				{
					if (CaretIndex == 0)
					{
						_sb.Remove(0, 1);
					}
				}
				e.Handled = true;
			}
			else
			{
				if (SelectionLength != 0)
					_sb.Clear();
				base.OnKeyDown(e);
			}
			PaintText();
		}

		protected override void OnTextInput(TextInputEventArgs e)
		{
			e.Handled = OnTextInput(e.Text);
		}

		private bool OnTextInput(string text)
		{
			if (_supressChanges) return true; //avoid recursive calls
			bool handledCorrectly = true;
			if (SelectionLength != 0)
			{
				_sb.Clear();
				_supressChanges = true; //avoid recursive calls
				SelectionStart = SelectionEnd = CaretIndex = 0;
				_supressChanges = false;
			}
			if (CaretIndex == 0) _sb.Insert(0, text);
			else _sb.Append(text);
			if (_sb.Length > Constants.MaxPasswordLength) //ensure the maximum length
			{
				_sb.Remove(Constants.MaxPasswordLength, _sb.Length - Constants.MaxPasswordLength);
				handledCorrectly = false; //should play beep sound not working on windows
			}
			PaintText();
			return handledCorrectly;
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
		{
			if (_supressChanges) return;

			if (e.Property.Name == nameof(CaretIndex))
			{
				CaretIndex = Text.Length;
			}
			else if (e.Property.Name == nameof(SelectionStart) || e.Property.Name == nameof(SelectionEnd))
			{
				var len = SelectionEnd - SelectionStart;
				if (len > 0)
				{
					SelectionStart = 0;
					SelectionEnd = Text.Length;
				}
				else
				{
					SelectionStart = SelectionEnd = CaretIndex;
				}
			}
			else
				base.OnPropertyChanged(e);
		}

		private void PaintText()
		{
			Password = _sb.ToString();
			Text = _displayText.Substring(0, _sb.Length);

			_supressChanges = true;
			try
			{
				//Text = Password; //for debugging
				CaretIndex = Text.Length;
			}
			finally
			{
				_supressChanges = false;
			}
		}

		protected override void OnGotFocus(GotFocusEventArgs e)
		{
			GenerateNewRandomSequence();
			base.OnGotFocus(e);
		}
	}
}
