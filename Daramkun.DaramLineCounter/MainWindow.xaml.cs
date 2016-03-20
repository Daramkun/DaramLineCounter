using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daramkun.DaramLineCounter
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow ()
		{
			InitializeComponent ();
		}

		private void PathBrowse_Click ( object sender, RoutedEventArgs e )
		{
			var dialog = new WPFFolderBrowser.WPFFolderBrowserDialog ();
			if ( dialog.ShowDialog () == false ) return;
			textBoxDestination.Text = dialog.FileName;
			Listing ();
		}

		private void IgnoreFileBrowse_Click ( object sender, RoutedEventArgs e )
		{
			var dialog = new Microsoft.Win32.OpenFileDialog () { Filter = ".gitignore File(.gitignore)|.gitignore" };
			if ( dialog.ShowDialog () == false ) return;
			comboBoxGitIgnore.Text = dialog.FileName;
			Listing ();
		}

		private void GitIgnore_IO_Click ( object sender, RoutedEventArgs e )
		{
			Process.Start ( "http://gitignore.io" );
		}

		private void comboBoxGitIgnore_KeyUp ( object sender, KeyEventArgs e )
		{
			if ( e.Key == Key.Enter )
				Listing ();
		}

		private void Listing ()
		{
			if ( textBoxDestination.Text == "" || !System.IO.Directory.Exists ( textBoxDestination.Text ) ) return;
			var list = new ConcurrentQueue<FileInfo> ();
			string path = textBoxDestination.Text;
			var gitignoreChecker = new GitIgnoreChecker ( comboBoxGitIgnore.SelectedIndex == 0 ? System.IO.Path.Combine ( path, ".gitignore" ) : comboBoxGitIgnore.Text );
			Parallel.ForEach<string> ( System.IO.Directory.GetFiles ( textBoxDestination.Text, "*.*", System.IO.SearchOption.AllDirectories ), ( filename ) =>
			{
				var proceed = FilenameProduce ( filename, path );
				if ( gitignoreChecker.IsValidFilename ( proceed ) )
					list.Enqueue ( new FileInfo ( filename ) );
			} );
			listViewFiles.ItemsSource = list;
			listViewFiles.SelectAll ();
		}

		private string FilenameProduce ( string filename, string path )
		{
			if ( filename.IndexOf ( path ) == 0 ) return filename.Substring ( path.Length );
			else return null;
		}

		private void listViewFiles_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			int count = 0;
			foreach ( FileInfo item in listViewFiles.SelectedItems )
				count += item.Count;
			textBlockTotalLines.Text = string.Format ( "{0:#,##0}", count );
		}

		private void listViewItem_MouseDoubleClick ( object sender, RoutedEventArgs e )
		{
			var fileInfo = ( sender as ListViewItem ).Content as FileInfo;
			if ( fileInfo.Count == 0 )
				fileInfo.Count = FileLineCountHelper.GetFileLineCount ( System.IO.Path.Combine ( fileInfo.Path, fileInfo.Filename ) );
			else fileInfo.Count = 0;
			fileInfo.ChangedCount ();
		}
	}
}
