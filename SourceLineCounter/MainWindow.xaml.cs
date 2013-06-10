using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskDialogInterop;

namespace SourceLineCounter
{
	public class CountCalculated
	{
		string filename = "", path = "";
		int count = 0;

		public string Filename { get { return filename; } set { filename = value; } }
		public int Count { get { return count; } set { count = value; } }
		public string Path { get { return path; } set { path = value; } }
	}

	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		ObservableCollection<string> extensionList = new ObservableCollection<string> ();
		ObservableCollection<string> ignoreList = new ObservableCollection<string> ();
		ObservableCollection<CountCalculated> countList = new ObservableCollection<CountCalculated> ();

		public MainWindow ()
		{
			InitializeComponent ();

			if ( File.Exists ( "directories.txt" ) )
			{
				using ( FileStream fs = new FileStream ( "directories.txt", FileMode.Open ) )
				{
					TextReader tr = new StreamReader ( fs );
					textboxBrowse.Text = tr.ReadLine ();
				}
			}

			if ( !File.Exists ( "extensions.txt" ) )
			{
				#region Extension List
				extensionList.Add ( "All Files (*.*)" );
				extensionList.Add ( "C (Include C Headers) (*.c, *.h)" );
				extensionList.Add ( "C++ (Include C Headers) (*.cpp, *.h)" );
				extensionList.Add ( "C++ (Include C++ Headers) (*.cpp, *.hpp)" );
				extensionList.Add ( "C++ (Include C and C++ Headers) (*.cpp, *.h, *.hpp)" );
				extensionList.Add ( "C/C++ (Include C and C++ Headers) (*.c, *.cpp, *.h, *.hpp)" );
				extensionList.Add ( "Visual Basic.NET (*.vb)" );
				extensionList.Add ( "C# (*.cs)" );
				extensionList.Add ( "Ruby (*.rb)" );
				extensionList.Add ( "Python (*.py)" );
				extensionList.Add ( "Javascript (*.js)" );
				extensionList.Add ( "PHP (*.php, *.php3)" );
				extensionList.Add ( "Java (*.java)" );
				extensionList.Add ( "HTML (*.htm, *.html)" );
				extensionList.Add ( "HTML and PHP (*.htm, *.html, *.php, *.php3)" );
				extensionList.Add ( "HTML and PHP and Javascript (*.htm, *.html, *.php, *.php3, *.js)" );
				extensionList.Add ( "HTML and JSP (*.htm, *.html, *.jsp)" );
				extensionList.Add ( "HTML and JSP and Javascript (*.htm, *.html, *.jsp, *.js)" );
				extensionList.Add ( "HTML and ASP (*.htm, *.html, *.asp)" );
				extensionList.Add ( "HTML and ASP and Javascript (*.htm, *.html, *.asp)" );
				extensionList.Add ( "HTML and ASP.NET (*.htm, *.html, *.aspx)" );
				extensionList.Add ( "HTML and ASP.NET and Javascript (*.htm, *.html, *.aspx, *.js)" );
				extensionList.Add ( "HTML and CGI (*.htm, *.html, *.cgi)" );
				extensionList.Add ( "HTML and CGI and Javascript (*.htm, *.html, *.cgi, *.js)" );
				#endregion
			}
			else
			{
				using ( FileStream fs = new FileStream ( "extensions.txt", FileMode.Open ) )
				{
					TextReader tr = new StreamReader ( fs );
					int count = int.Parse ( tr.ReadLine () );
					for ( int i = 0; i < count; i++ )
						extensionList.Add ( tr.ReadLine () );
				}
			}
			comboExtensions.ItemsSource = extensionList;
			comboExtensions.SelectedIndex = 0;

			ignoreList.Add ( "None" );
			ignoreList.Add ( "Makefile" );
			ignoreList.Add ( "Visual Studio Solution and EXE files" );
			ignoreList.Add ( "Xcode Project" );
			ignoreList.Add ( "Eclipse Project and bin folder" );
			comboIgnore.ItemsSource = ignoreList;
			comboIgnore.SelectedIndex = 0;

			if ( File.Exists ( "ignore.txt" ) )
			{
				using ( FileStream fs = new FileStream ( "ignore.txt", FileMode.Open ) )
				{
					TextReader tr = new StreamReader ( fs );
					textboxIgnore.Text = tr.ReadLine ();
				}
			}


			listCounts.ItemsSource = countList;
		}

		private void Window_Closing ( object sender, System.ComponentModel.CancelEventArgs e )
		{
			using ( FileStream fs = new FileStream ( "directories.txt", FileMode.OpenOrCreate ) )
			{
				TextWriter tw = new StreamWriter ( fs );
				tw.WriteLine ( textboxBrowse.Text );
			}

			using ( FileStream fs = new FileStream ( "ignore.txt", FileMode.OpenOrCreate ) )
			{
				TextWriter tw = new StreamWriter ( fs );
				tw.WriteLine ( textboxIgnore.Text );
			}

			using ( FileStream fs = new FileStream ( "extensions.txt", FileMode.OpenOrCreate ) )
			{
				TextWriter tw = new StreamWriter ( fs );
				tw.WriteLine ( extensionList.Count.ToString () );
				foreach ( string extension in extensionList )
					tw.WriteLine ( extension );
			}
		}

		private void buttonBrowse_Click ( object sender, RoutedEventArgs e )
		{
			System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog ();
			dialog.SelectedPath = textboxBrowse.Text;
			if ( dialog.ShowDialog () == System.Windows.Forms.DialogResult.Cancel ) return;
			textboxBrowse.Text = dialog.SelectedPath;
		}

		private void buttonAddExtension_Click ( object sender, RoutedEventArgs e )
		{
			if ( textboxAddExtension.Text.Trim () == "" )
			{
				TaskDialogOptions config = new TaskDialogOptions ();
				config.Owner = this;
				config.Title = "Error";
				config.MainInstruction = "File Extension Textbox is empty.";
				config.Content = "Please input File Extension in File Extension Textbox.";
				config.ExpandedInfo = "Format: Name (*.FileExtension, *.FileExtension, ...)";
				config.CustomButtons = new string [] { "&OK" };
				config.MainIcon = VistaTaskDialogIcon.Error;
				TaskDialogResult res = TaskDialog.Show ( config );
				return;
			}
			extensionList.Add ( textboxAddExtension.Text );
			textboxAddExtension.Text = "";
		}

		private void buttonRemoveExtension_Click ( object sender, RoutedEventArgs e )
		{
			TaskDialogOptions config = new TaskDialogOptions ();

			if ( comboExtensions.SelectedIndex < 0 )
			{
				config.Owner = this;
				config.Title = "Error";
				config.MainInstruction = "File Extension Combo Box Item is not selected";
				config.Content = "Please select File Extension Item for Remove item.";
				config.CustomButtons = new string [] { "&OK" };
				config.MainIcon = VistaTaskDialogIcon.Error;
				TaskDialogResult res = TaskDialog.Show ( config );
				return;
			}
			else
			{
				config.Owner = this;
				config.Title = "Question";
				config.MainInstruction = "Do you want to remove selected File Extension?";
				config.Content = "If you click \"Yes\" button, Remove selected File Extension.";
				config.CustomButtons = new string [] { "&Yes", "&No" };
				config.MainIcon = VistaTaskDialogIcon.Shield;
				TaskDialogResult res = TaskDialog.Show ( config );
				if ( res.CustomButtonResult == 0 )
					extensionList.Remove ( comboExtensions.SelectedItem as string );
			}
		}

		string [] GetExtensions (string extension)
		{
			int lastIndex = extension.LastIndexOf ( '(' );
			string temp = extension.Substring ( lastIndex, extension.Length - lastIndex );
			temp = temp.Replace ( "(", "" );
			temp = temp.Replace ( ")", "" );
			string[] temp2 = temp.Split ( ',' );
			for ( int i = 0; i < temp2.Length; i++ )
				temp2 [ i ] = temp2 [ i ].Trim ();
			return temp2;
		}

		bool IsIgnoreList ( string filename, string extension, int ignoreIndex, string ignoreText )
		{
			bool extensions = false;
			foreach ( string ext in GetExtensions ( extension ) )
				if ( ext == "*.*" )
					extensions = true;

			if ( !extensions ) return false;

			List<string> ignoreList = new List<string> ();

			switch ( ignoreIndex )
			{
				case 1:
					ignoreList.Add ( "makefile" );
					ignoreList.Add ( "mk" );
					break;
				case 2:
					ignoreList.Add ( ".sln" );
					ignoreList.Add ( ".suo" );
					ignoreList.Add ( ".pdb" );
					ignoreList.Add ( ".exe" );
					ignoreList.Add ( ".config" );
					ignoreList.Add ( ".dll" );
					ignoreList.Add ( ".log" );
					ignoreList.Add ( ".csproj" );
					ignoreList.Add ( ".vcxproj" );
					ignoreList.Add ( ".vbproj" );
					ignoreList.Add ( ".resx" );
					ignoreList.Add ( ".settings" );
					ignoreList.Add ( ".nupkg" );
					ignoreList.Add ( ".nuspec" );
					ignoreList.Add ( ".filters" );
					ignoreList.Add ( ".user" );
					ignoreList.Add ( ".sdf" );
					ignoreList.Add ( ".ilk" );
					ignoreList.Add ( ".tlog" );
					ignoreList.Add ( ".obj" );
					ignoreList.Add ( ".idb" );
					ignoreList.Add ( ".lastbuildstate" );
					ignoreList.Add ( ".xap" );
					ignoreList.Add ( ".appx" );
					ignoreList.Add ( ".appxupload" );
					ignoreList.Add ( ".cer" );
					ignoreList.Add ( ".appxsym" );
					ignoreList.Add ( ".ps1" );
					ignoreList.Add ( ".psd1" );
					ignoreList.Add ( ".pfx" );
					ignoreList.Add ( ".appxmanifest" );
					ignoreList.Add ( ".intermediate" );
					ignoreList.Add ( ".resfiles" );
					ignoreList.Add ( ".backup" );
					ignoreList.Add ( ".resources" );
					ignoreList.Add ( ".cache" );
					ignoreList.Add ( ".cachefile" );
					ignoreList.Add ( ".sample" );
					ignoreList.Add ( ".manifest" );
					ignoreList.Add ( ".lastcodeanalysissucceeded" );
					break;
				case 3:
					ignoreList.Add ( ".classpath" );
					ignoreList.Add ( ".project" );
					ignoreList.Add ( ".cfg" );
					ignoreList.Add ( ".properties" );
					ignoreList.Add ( ".prefs" );
					ignoreList.Add ( ".class" );
					ignoreList.Add ( ".bin" );
					ignoreList.Add ( ".o" );
					ignoreList.Add ( ".elf" );
					ignoreList.Add ( ".dep" );
					ignoreList.Add ( ".exe" );
					break;
				case 4:
					break;
			}

			foreach ( string iglist in ignoreText.Split ( ',' ) )
				ignoreList.Add ( iglist.Trim () );

			foreach ( string ig in ignoreList )
			{
				if ( System.IO.Path.GetExtension ( filename ).ToLower () == ig )
					return true;
				if ( filename.Trim ().ToLower () == ig )
					return true;
			}

			return false;
		}

		private delegate void InvokeDelegate ();

		private async void buttonCountStart_Click ( object sender, RoutedEventArgs e )
		{
			countList.Clear ();

			string extension = comboExtensions.SelectedItem as string;
			string browseText = textboxBrowse.Text;

			int ignoreIndex = comboIgnore.SelectedIndex;
			string ignoreText = textboxIgnore.Text;

			int count = 0;
			string [] filters = GetExtensions ( extension );
			List<string> files = new List<string> ();

			comboExtensions.IsEnabled = comboIgnore.IsEnabled = textboxBrowse.IsEnabled = textboxIgnore.IsEnabled = 
				textboxAddExtension.IsEnabled = buttonAddExtension.IsEnabled = buttonBrowse.IsEnabled = 
				buttonCountStart.IsEnabled = buttonRemoveExtension.IsEnabled = false;

			await Task.Run ( () =>
			{
				foreach ( string filter in filters )
				{
					try
					{
						string [] filesArray = Directory.GetFiles ( browseText, filter, SearchOption.AllDirectories );
						foreach ( string file in filesArray )
							files.Add ( file );
					}
					catch { }
				}

				foreach ( string f in files )
				{
					Dispatcher.BeginInvoke ( new InvokeDelegate ( () =>
					{
						this.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo ();
						this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
						this.TaskbarItemInfo.ProgressValue = ( ++count / ( float ) files.Count );
					} ) );
					if ( IsIgnoreList ( f, extension, ignoreIndex, ignoreText ) ) continue;
					int len = File.ReadAllLines ( f ).Length;
					Dispatcher.BeginInvoke ( new InvokeDelegate ( () =>
					{
						countList.Add ( new CountCalculated () { Filename = System.IO.Path.GetFileName ( f ), Count = len, Path = System.IO.Path.GetDirectoryName ( f ) } );
					} ) );
				}
			} );

			foreach ( CountCalculated selected in countList )
				listCounts.SelectedItems.Add ( selected );

			await Task.Run ( () => { GC.Collect (); } );

			comboExtensions.IsEnabled = comboIgnore.IsEnabled = textboxBrowse.IsEnabled = textboxIgnore.IsEnabled =
				textboxAddExtension.IsEnabled = buttonAddExtension.IsEnabled = buttonBrowse.IsEnabled =
				buttonCountStart.IsEnabled = buttonRemoveExtension.IsEnabled = true;
		}

		private void listCounts_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			int total = 0;

			IList selectedItem = listCounts.SelectedItems;
			foreach ( CountCalculated item in selectedItem )
				total += item.Count;

			totalCount.Text = String.Format ( "{0:0,0}", total );
		}

		private void listCounts_Click_1 ( object sender, RoutedEventArgs e )
		{

		}
	}
}
