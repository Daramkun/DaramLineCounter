using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Daramkun.DaramLineCounter
{
	public class FileInfo : INotifyPropertyChanged, IComparer<FileInfo>
	{
		public FileInfo ( string filename )
		{
			Filename = System.IO.Path.GetFileName ( filename );
			Path = System.IO.Path.GetDirectoryName ( filename );
			if ( FileLineCountHelper.IsText ( filename, 8096 ) )
				Count = FileLineCountHelper.GetFileLineCount ( filename );
		}

		public string Filename { get; set; }
		public int Count { get; set; }
		public string Path { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public void ChangedCount () { if ( PropertyChanged != null ) PropertyChanged ( this, new PropertyChangedEventArgs ( nameof ( Count ) ) ); }

		public int Compare ( FileInfo x, FileInfo y )
		{
			return x.Count - y.Count;
		}
	}
}
