/******************************************************************************
	Copyright 2003-2004 Hamid Qureshi and Unruled Boy 
	iOfficeMail.Net is free software; you can redistribute it and/or modify
	it under the terms of the Lesser GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	iOfficeMail.Net is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	Lesser GNU General Public License for more details.

	You should have received a copy of the Lesser GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
/*******************************************************************************/

/*
*Name:			iOfficeMailSMTP.Mail.Utility
*Function:		Utility
*Author:		Hamid Qureshi
*Created:		2003/8
*Modified:		2004/5/30 15:04 GMT+8 by Unruled Boy
*Description:
*Changes:		
*				2004/5/30 15:04 GMT+8 by Unruled Boy
*					1.Added all description to all functions
*				2004/5/25 13:55 GMT+8 by Unruled Boy
*					1.Rewrote the DecodeText function using Regular Expression
*				2004/5/17 14:20 GMT+8 by Unruled Boy
*					1.Added ParseFileName
*				2004/4/29 19:05 GMT+8 by Unruled Boy
*					1.Adding ReadPlainTextFromFile function
*				2004/4/28 19:06 GMT+8 by Unruled Boy
*					1.Rewriting the Decode method
*				2004/3/29 12:25 GMT+8 by Unruled Boy
*					1.GetMimeType support for MONO
*					2.cleaning up the names of variants
*/
using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace iOfficeMail.MIMEParser
{
	/// <summary>
	/// Summary description for Utility.
	/// </summary>
	public class Utility
	{
		private static bool m_blnLog=false;
		private static string m_strLogFile = "iOfficeMail.log";

		public Utility()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		//		public static string[] SplitText(string strText, string strSplitter)
		//		{
		//			string []segments=new string[0];
		//			int indexOfstrSplitter=strText.IndexOf(strSplitter);
		//			if(indexOfstrSplitter!=-1)
		//			{
		//
		//			}
		//			return segments;
		//		}
		//

		/// <summary>
		/// Verifies whether the file is of picture type or not
		/// </summary>
		/// <param name="strFile">File to be verified</param>
		/// <returns>True if picture file, false if not</returns>
		public static bool IsPictureFile(string strFile)
		{
			try
			{
				if(strFile!=null&&strFile!="")
				{
					strFile=strFile.ToLower();
					if(strFile.EndsWith(".jpg")||strFile.EndsWith(".bmp")||strFile.EndsWith(".ico")||strFile.EndsWith(".gif")||strFile.EndsWith(".png"))
						return true;
					else
						return false;
				}
				else
					return false;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Parse date time info from MIME header
		/// </summary>
		/// <param name="strDate">Encoded MIME date time</param>
		/// <returns>Decoded date time info</returns>
		public static string ParseEmailDate(string strDate)
		{
			string strRet=strDate.Trim();
			int indexOfTag=strRet.IndexOf(",");
			if(indexOfTag!=-1)
			{
				strRet=strRet.Substring(indexOfTag+1);
			}

			strRet=QuoteText(strRet,"+");
			strRet=QuoteText(strRet,"-");
			strRet=QuoteText(strRet,"GMT");
			strRet=QuoteText(strRet,"CST");
			return strRet.Trim();
		}

		/// <summary>
		/// Quote the text according to a tag
		/// </summary>
		/// <param name="strText">Text to be quoted</param>
		/// <param name="strTag">Quote tag</param>
		/// <returns>Quoted Text</returns>
		public static string QuoteText(string strText, string strTag)
		{
			int indexOfTag=strText.IndexOf(strTag);
			if(indexOfTag!=-1)
				return strText.Substring(0,indexOfTag-1);
			else
				return strText;
		}

		/// <summary>
		/// Parse file name from MIME header
		/// </summary>
		/// <param name="strHeader">MIME header</param>
		/// <returns>Decoded file name</returns>
		public static string ParseFileName(string strHeader)
		{
			string strTag;
			strTag="filename=";
			int intPos=strHeader.ToLower().IndexOf(strTag);
			if(intPos==-1)
			{
				strTag="name=";
				intPos=strHeader.ToLower().IndexOf(strTag);
			}
			string strRet;
			if(intPos!=-1)
			{
				strRet=strHeader.Substring(intPos+strTag.Length);
				intPos=strRet.ToLower().IndexOf(";");
				if(intPos!=-1)
					strRet=strRet.Substring(1,intPos-1);
				strRet=RemoveQuote(strRet);
			}
			else
				strRet="";

			return strRet;
		}

		/// <summary>
		/// Parse email address from MIME header
		/// </summary>
		/// <param name="strEmailAddress">MIME header</param>
		/// <param name="strUser">Decoded user name</param>
		/// <param name="strAddress">Decoded email address</param>
		/// <returns>True if decoding succeeded, false if failed</returns>
		public static bool ParseEmailAddress(string strEmailAddress,ref string strUser, ref string strAddress)
		{
			int indexOfAB=strEmailAddress.Trim().LastIndexOf("<");
			int indexOfEndAB=strEmailAddress.Trim().LastIndexOf(">");
			strUser=strEmailAddress;
			strAddress=strEmailAddress;
			if(indexOfAB>=0&&indexOfEndAB>=0)
			{
				if(indexOfAB>0)
				{
					strUser=strUser.Substring(0,indexOfAB-1);
					//					strUser=strUser.Substring(0,indexOfAB-1).Trim('\"');
					//					if(strUser.IndexOf("\"")>=0)
					//					{
					//						strUser=strUser.Substring(1,strUser.Length-1);
					//					}
				}
				strUser=strUser.Trim();
				strUser=strUser.Trim('\"');
				strAddress=strAddress.Substring(indexOfAB+1,indexOfEndAB-(indexOfAB+1));
			}
			strUser=strUser.Trim();
			strUser=DecodeText(strUser);
			strAddress=strAddress.Trim();

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bytContent"></param>
		/// <param name="strFile"></param>
		/// <returns></returns>
		public static bool SaveByteContentToFile(byte[] bytContent,string strFile)
		{
			try
			{
				if(File.Exists(strFile))
					File.Delete(strFile);
				FileStream fs=File.Create(strFile);
				fs.Write(bytContent,0,bytContent.Length);
				fs.Close();
				return true;
			}
			catch(Exception e)
			{
				Utility.LogError("SaveByteContentToFile():"+e.Message);
				return false;
			}
		}

		public static bool SavePlainTextToFile(string strFile, string strText, bool blnReplaceExists)
		{
			try
			{
				bool blnRet=true;

				if(File.Exists(strFile))
				{
					if(blnReplaceExists)
						File.Delete(strFile);
					else
						blnRet=false;
				}

				if(blnRet==true)
				{
					StreamWriter sw=File.CreateText(strFile);
					sw.Write(strText);
					sw.Close();
				}

				return blnRet;
			}
			catch(Exception e)
			{
				Utility.LogError("SavePlainTextToFile():"+e.Message);
				return false;
			}
		}

		public static bool ReadPlainTextFromFile(string strFile, ref string strText)
		{
			if(File.Exists(strFile))
			{
				StreamReader fs=new StreamReader(strFile);
				strText=fs.ReadToEnd();
				fs.Close();
				return true;
			}
			else
				return false;
		}

/*		/// <summary>
		/// Sepearte header name and header value
		/// </summary>
		/// <param name="strRawHeader"></param>
		/// <returns></returns>
		public static string[] GetHeadersValue(string strRawHeader)
		{
			if(strRawHeader==null)
				throw new ArgumentNullException("strRawHeader","Argument was null");

			string []array=new string[2]{"",""};
			int indexOfColon=strRawHeader.IndexOf(":");			

			try
			{
				array[0]=strRawHeader.Substring(0,indexOfColon).Trim();
				array[1]=strRawHeader.Substring(indexOfColon+1).Trim();
			}
			catch(Exception){}

			return array;
		}*/

		public static string GetQuotedValue(string strText, string strSplitter, string strTag)
		{
			if(strText==null)
				throw new ArgumentNullException("strText","Argument was null");

			string []array=new string[2]{"",""};
			int indexOfstrSplitter=strText.IndexOf(strSplitter);			

			try
			{
				array[0]=strText.Substring(0,indexOfstrSplitter).Trim();
				array[1]=strText.Substring(indexOfstrSplitter+1).Trim();
				int pos=array[1].IndexOf("\"");
				if(pos!=-1)
				{
					int pos2=array[1].IndexOf("\"",pos+1);
					array[1]=array[1].Substring(pos+1,pos2-pos-1);
				}
			}
			catch(Exception){}

			//return array;
			if(array[0].ToLower()==strTag.ToLower())
				return array[1].Trim();
			else
				return null;

/*			string []array=null;
			try
			{
				array=Regex.Split(strText,strSplitter);
				//return array;
				if(array[0].ToLower()==strTag.ToLower())
					return RemoveQuote(array[1].Trim());
				else
					return null;
			}
			catch
			{return null;}*/
		}

		public static string Change(string strText,string charset)
		{
			if (charset==null || charset=="")
				return strText;
			byte[] b=Encoding.Default.GetBytes(strText);
			return new string(Encoding.GetEncoding(charset).GetChars(b));
		}

		public static string RemoveNonB64(string strBuffer)
		{
			return strBuffer.Replace("\0","");
		}

		public static string RemoveWhiteBlanks(string strBuffer)
		{
			return strBuffer.Replace("\0","").Replace("\r\n","");
		}

		public static string RemoveQuote(string strBuffer)			
		{
			string strRet=strBuffer;
			if(strRet.StartsWith("\""))
				strRet=strRet.Substring(1);
			if(strRet.EndsWith("\""))
				strRet=strRet.Substring(0,strRet.Length-1);
			return strRet;
		}

		public static string DecodeLine(string strSrc)
		{
			return DecodeText(RemoveWhiteBlanks(strSrc));
		}

		private static bool IsValidMIMEText(string strSrc)
		{
			int intPos=strSrc.IndexOf("=?");
			return (intPos!=-1&&strSrc.IndexOf("?=",intPos+6)!=-1&&strSrc.Length>7);
		}

		public static string DecodeText(string strSrc)
		{
			/*try
			{
				string strRet="";
				string strBody="";
				MatchCollection mc=Regex.Matches(strSrc,@"\=\?(?<Charset>\S+)\?(?<Encoding>\w)\?(?<Content>\S+)\?\=");

				for(int i=0;i<mc.Count;i++)
				{
					if(mc[i].Success)
					{
						strBody=mc[i].Groups["Content"].Value;

						switch(mc[i].Groups["Encoding"].Value.ToUpper())
						{
							case "B":
								strBody=deCodeB64s(strBody,mc[i].Groups["Charset"].Value);
								break;
							case "Q":
								strBody=DecodeQP.ConvertHexContent(strBody);//, m.Groups["Charset"].Value);
								break;
							default:
								break;
						}
						strRet+=strBody;
					}
					else
					{
						strRet+=mc[i].Value;
					}
				}
				return strRet;
			}
			catch
			{return strSrc;}*/

			try
			{
				string strRet="";
				string[] strParts=Regex.Split(strSrc,"\r\n");
				string strBody="";
				const string strRegEx=@"\=\?(?<Charset>\S+)\?(?<Encoding>\w)\?(?<Content>\S+)\?\=";
				Match m=null;

				for(int i=0;i<strParts.Length;i++)
				{
					m = Regex.Match(strParts[i], strRegEx);
					if(m.Success)
					{
						strBody=m.Groups["Content"].Value;

						switch(m.Groups["Encoding"].Value.ToUpper())
						{
							case "B":
								strBody=deCodeB64s(strBody,m.Groups["Charset"].Value);
								break;
							case "Q":
								strBody=DecodeQP.ConvertHexContent(strBody);//, m.Groups["Charset"].Value);
								break;
							default:
								break;
						}
						strRet+=strBody;
					}
					else
					{
						strRet+=strParts[i];
					}
				}
				return strRet;
			}
			catch
			{return strSrc;}

/*		
		{
			try
			{
				if(strSrc!=null&&strSrc!="")
				{
					if(IsValidMIMEText(strSrc))
					{
						//position at the end of charset
						int intPos=strSrc.IndexOf("=?");
						int intPos2=strSrc.IndexOf("?",intPos+2);
						if(intPos2>3)
						{
							string strCharset=strSrc.Substring(2,intPos2-2);
							string strEncoding=strSrc.Substring(intPos2+1,1);
							int intPos3=strSrc.IndexOf("?=",intPos2+3);
							string strBody=strSrc.Substring(intPos2+3,intPos3-intPos2-3);
							string strHead="";
							if(intPos>0)
							{
								strHead=strSrc.Substring(0,intPos-1);
							}
							string strEnd="";
							if(intPos3<strSrc.Length-2)
							{
								strEnd=strSrc.Substring(intPos3+2);
							}
							switch(strEncoding.ToUpper())
							{
								case "B":
									strBody=deCodeB64s(strBody);
									break;
								case "Q":
									strBody=DecodeQP.ConvertHexContent(strBody);
									break;
								default:
									break;
							}
							strSrc=strHead+strBody+strEnd;
							if(IsValidMIMEText(strSrc))
								return DecodeText(strSrc);
							else
								return strSrc;
						}
						else
						{return strSrc;}
					}
					else
					{return strSrc;}
				}
				else
				{return strSrc;}
			}
			catch
			{return strSrc;}*/
		}
		
		public static string deCodeB64s(string strSrc)
		{
			return Encoding.Default.GetString(deCodeB64(strSrc));
		}
		
		public static string deCodeB64s(string strSrc,string strEncoding)
		{
			if(strEncoding.ToLower()=="ISO-8859-1".ToLower())
				return deCodeB64s(strSrc);
			else
				return Encoding.GetEncoding(strEncoding).GetString(deCodeB64(strSrc));				
		}
		
		private static byte []deCodeB64(string strSrc)
		{
			byte[] by=null;
			try
			{ 
				if(strSrc!="")
				{
					by=Convert.FromBase64String(strSrc); 
					//strSrc=Encoding.Default.GetString(by);
				}
			} 
			catch(Exception e) 
			{
				by=Encoding.Default.GetBytes("\0");
				LogError("deCodeB64():"+e.Message);
			}
			return by;
		}

		/// <summary>
		/// Turns file logging on and off.
		/// </summary>
		/// <remarks>Comming soon.</remarks>
		public static bool Log
		{
			get
			{
				return m_blnLog;
			}
			set
			{
				m_blnLog = value;
			}
		}

		internal static void LogError(string strText) 
		{
			//Log=true;
			if(Log)
			{
				FileInfo file = null;
				FileStream fs = null;
				StreamWriter sw = null;
				try
				{
					file = new FileInfo(m_strLogFile);
					sw = file.AppendText();
					//fs = new FileStream(m_strLogFile, FileMode.OpenOrCreate, FileAccess.Write);
					//sw = new StreamWriter(fs);
					sw.WriteLine(DateTime.Now);
					sw.WriteLine(strText);
					sw.WriteLine("\r\n");
					sw.Flush();
				}
				finally
				{
					if(sw != null)
					{
						sw.Close();
						sw = null;
					}
					if(fs != null)
					{
						fs.Close();
						fs = null;
					}
					
				}
			}
		}

		public static bool IsQuotedPrintable(string strText)
		{
			if(strText!=null)
				return (strText.ToLower()=="quoted-printable".ToLower());
			else
				return false;
		}

		public static bool IsBase64(string strText)
		{
			if(strText!=null)
				return (strText.ToLower()=="base64".ToLower());
			else
				return false;
		}

		public static string[] SplitOnSemiColon(string strText)
		{
			if(strText==null)
				throw new ArgumentNullException("strText","Argument was null");

			string []array=null;
			int indexOfColon=strText.IndexOf(";");			

			if(indexOfColon<0)
			{
				array=new string[1];
				array[0]=strText;
				return array;
			}
			else
			{
				array=new string[2];
			}

			try
			{
				array[0]=strText.Substring(0,indexOfColon).Trim();
				array[1]=strText.Substring(indexOfColon+1).Trim();
			}
			catch(Exception){}

			return array;
		}

		public static bool IsNotNullText(string strText)
		{
			try
			{
				return (strText!=null&&strText!="");
			}
			catch
			{
				return false;
			}
		}

		public static bool IsNotNullTextEx(string strText)
		{
			try
			{
				return (strText!=null&&strText.Trim()!="");
			}
			catch
			{
				return false;
			}
		}

		public static bool IsOrNullTextEx(string strText)
		{
			try
			{
				return (strText==null||strText.Trim()=="");
			}
			catch
			{
				return false;
			}
		}

	}
}

