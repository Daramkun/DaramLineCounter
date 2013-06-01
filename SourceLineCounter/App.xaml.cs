using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SourceLineCounter
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup ( StartupEventArgs e )
		{
			RenderOptions.ClearTypeHintProperty.OverrideMetadata ( typeof ( FrameworkElement ),
				new FrameworkPropertyMetadata { DefaultValue = ClearTypeHint.Enabled } );
			TextOptions.TextFormattingModeProperty.OverrideMetadata ( typeof ( FrameworkElement ),
				new FrameworkPropertyMetadata { DefaultValue = TextFormattingMode.Display } );
			base.OnStartup ( e );
		}
	}
}
