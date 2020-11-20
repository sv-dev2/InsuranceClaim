using SV.Domain.Code;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Domain.Code
{
	public class NfCmdLineMgr
	{
		#region The Singleton definition
		// the one and only Singleton instance. 
		private static readonly NfCmdLineMgr instance = new NfCmdLineMgr();

		// private constructor. 
		private NfCmdLineMgr()
		{ 
			Debug = false;
			ExitApp = false;
			AutoRun = false;
			PreviewMode = ClmPreviewMode.ClmPreviewModeNone;
			PreviewId = 0;
		}

		// gets the instance of the singleton object
		public static NfCmdLineMgr Instance
		{
			get { return instance; }
		}

		#endregion

		public enum NfAppParamChoice
		{
			NfAppParamChoiceYes,
			NfAppParamChoiceNo,
			NfAppParamChoiceUnchanged,
		}

        public enum ClmPreviewMode
        {
            ClmPreviewModeNone,
            ClmPreviewModeStepInstructions,
            ClmPreviewModeValves,
        }

        public bool Debug;
		public bool ExitApp;
		public bool AutoRun;
		public int PreviewId;
		public ClmPreviewMode PreviewMode = ClmPreviewMode.ClmPreviewModeNone;

		private void Usage()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Insurance Batch Control");
			sb.Append("------------------------------------------------------------");
			sb.Append("Parameters ");
			sb.Append("  -Run								Auto run application");
			sb.Append("  -d									Debug");
            sb.Append("  -p	<preview consumption ID>		Preview Consumption ID");
            sb.Append("  -v	<preview valve list ID>		    Preview ValvePositionMap Processing");
            Console.WriteLine(sb.ToString());
		}

		private string NextGet(ref int i, string[] args, int count)
		{
			i++;
			if(i < args.Count())
				return args[i];
			else
				Usage();

			return "Unknown";
		}

		public void Init()
		{
			RunMgr.Instance.Init();
		}

		public bool MakeChoice(NfAppParamChoice choice)
		{
			if(choice == NfAppParamChoice.NfAppParamChoiceYes)
				return true;

			return false;
		}

		public bool MakeChoice(NfAppParamChoice choice, bool curValue)
		{
			switch(choice)
			{
				case NfAppParamChoice.NfAppParamChoiceYes:
					return true;
				case NfAppParamChoice.NfAppParamChoiceNo:
					return false;
				default:
					break;
			}

			return curValue;
		}

		public int Process(string[] args)
		{
			Logger.Instance.Debug("Command count: " + args.Count());
			for(var i = 0; i < args.Count(); i++)
			{
				if(args[i].Equals("-d"))
					Logger.Instance.Severity = LogSeverity.Debug;
			}

			for(var i = 1; i < args.Count(); i++)
			{
				string val = args[i].Trim();
				if(!String.IsNullOrEmpty(val))
				{
					Logger.Instance.Info("Processing: '" + val + "'");
					switch(val)
					{
						case "-Run":
							AutoRun = true;
							break;
						case "-d":
							Debug = true;
							break;
                        case "-p":
                            PreviewId = int.Parse(args[++i].Trim());
                            PreviewMode = ClmPreviewMode.ClmPreviewModeStepInstructions;
                            break;
                        case "-v":
                            PreviewId = int.Parse(args[++i].Trim());
                            PreviewMode = ClmPreviewMode.ClmPreviewModeValves;
                            break;
                        case "-Exit":
							ExitApp = true;
							break;
						default:
							Usage();
							return 0;
					}
				}
			}

			return 0;
		}
	}
}
