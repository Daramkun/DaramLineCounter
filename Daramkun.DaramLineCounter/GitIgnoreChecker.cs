using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Daramkun.DaramLineCounter
{
	public class GitIgnoreChecker
	{
		//List<Regex> lists = new List<Regex> ();
		Regex regex;

		public GitIgnoreChecker ( string filename )
		{
			if ( File.Exists ( filename ) )
			{
				Stream stream = stream = File.Open ( filename, FileMode.Open, FileAccess.Read );
				Initializer ( stream );
			}
			else if ( filename.Substring ( 0, 4 ) == "http" || filename.Substring ( 0, 5 ) == "https" )
			{
				WebRequest request = WebRequest.CreateHttp ( filename );
				WebResponse response = request.GetResponse ();
				Stream stream = response.GetResponseStream ();
				Initializer ( stream );
				stream.Dispose ();
				response.Dispose ();
			}
		}

		public GitIgnoreChecker ( Stream stream )
		{
			Initializer ( stream );
		}

		private void Initializer ( Stream stream )
		{
			StringBuilder builder = new StringBuilder ();
			builder.Append ( "((.*)\\/.git\\/(.*))|" );
			using ( StreamReader reader = new StreamReader ( stream, true ) )
			{
				while ( !reader.EndOfStream )
				{
					var line = reader.ReadLine ().Trim ();
					if ( line.Length == 0 || line [ 0 ] == '#' )
						continue;
					line = $"(.*)/{line.Replace ( "/", "\\/" ).Replace ( "\\", "\\\\" ).Replace ( "*", "(.*)" )}(.*)";
					builder.AppendFormat ( "({0})|", line );
				}
			}
			builder.Remove ( builder.Length - 1, 1 );
			regex = new Regex ( builder.ToString () );
		}

		public bool IsValidFilename ( string proceedFilename )
		{
			proceedFilename = proceedFilename.Replace ( '\\', '/' );
			if ( regex.IsMatch ( proceedFilename ) )
				return false;
			return true;
		}
	}
}
