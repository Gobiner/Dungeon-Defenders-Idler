using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DungeonDefendersIdler
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const uint WM_KEYDOWN = 0x0100;
		private const uint WM_CHAR = 0x0102;
		private const uint WM_KEYUP = 0x0101;
		private const uint CH_G = 0x47;

		private Predicate<ManagedWinapi.Windows.SystemWindow> dundefIdentifier = x => x.ClassName == "LaunchUnrealUWindowsClient" && x.Title == "Dungeon Defenders";

		private bool IsIdling = false;

		private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer() { Enabled = true, Interval = 3000, };

		System.Windows.Forms.NotifyIcon notificationIcon = new System.Windows.Forms.NotifyIcon();
		
		public MainWindow()
		{
			InitializeComponent();

			this.Icon = BitmapFrame.Create(new Uri("pack://application:,,,./Resources/phoenician_g.ico", UriKind.RelativeOrAbsolute));
			notificationIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,./Resources/phoenician_g.ico")).Stream);
			notificationIcon.Visible = true;
			notificationIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
			notificationIcon.ContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
				new System.Windows.Forms.ToolStripMenuItem("Restore", null, delegate(object sender, EventArgs e)
				{
					this.Show();
               		this.WindowState = WindowState.Normal;
				}),
				new System.Windows.Forms.ToolStripMenuItem("Exit", null, delegate(object sender, EventArgs e)
				{
					this.Close();
				}),
			});

			notificationIcon.DoubleClick += delegate(object sender, EventArgs e)
			{
				this.Show();
				this.WindowState = WindowState.Normal;
			};

			this.StateChanged += delegate(object sender, EventArgs e)
			{
				if (this.WindowState == WindowState.Minimized)
				{
					this.Hide();
				}
			};

			IdleWhenInForeground.IsChecked = DungeonDefendersIdler.Properties.Settings.Default.IdleWhenInForeground;
			IdleWhenInForeground.Checked += new RoutedEventHandler(IdleWhenInForeground_Checked);
			IdleWhenInForeground.Unchecked += new RoutedEventHandler(IdleWhenInForeground_Checked);

			SetGoButtonBackgroundColor();
			GoButton.Click += new RoutedEventHandler(GoButton_Click);

			timer.Tick += new EventHandler(timer_Tick);
		}

		void timer_Tick(object sender, EventArgs e)
		{
			if(IsIdling)
				SendGToDundef();
		}

		void IdleWhenInForeground_Checked(object sender, RoutedEventArgs e)
		{
			DungeonDefendersIdler.Properties.Settings.Default.IdleWhenInForeground = IdleWhenInForeground.IsChecked.Value;
			DungeonDefendersIdler.Properties.Settings.Default.Save();
		}

		void GoButton_Click(object sender, RoutedEventArgs e)
		{
			IsIdling = !IsIdling;
			SetGoButtonBackgroundColor();
			FocusElement.Focus();
		}

		private void SetGoButtonBackgroundColor()
		{
			if(IsIdling)
			{
				GoButton.Background = Brushes.LightGreen;
				GoButton.Content = "Running";
			}
			else
			{
				GoButton.Background = Brushes.Pink;
				GoButton.Content = "Stopped";
			}
		}

		private void SendGToDundef()
		{
			var dundef = ManagedWinapi.Windows.SystemWindow.FilterToplevelWindows(dundefIdentifier).SingleOrDefault();
			if(dundef == null) return;

			if(!dundefIdentifier(ManagedWinapi.Windows.SystemWindow.ForegroundWindow))
			{
				dundef.SendSetMessage(WM_KEYDOWN, CH_G);
				dundef.SendSetMessage(WM_CHAR, CH_G);
				dundef.SendSetMessage(WM_KEYUP, CH_G);
			}
			else
			{
				if(IdleWhenInForeground.IsChecked.HasValue && IdleWhenInForeground.IsChecked.Value)
				{
					dundef.SendSetMessage(WM_KEYDOWN, CH_G);
					dundef.SendSetMessage(WM_CHAR, CH_G);
					dundef.SendSetMessage(WM_KEYUP, CH_G);
				}
			}
		}
	}
}
