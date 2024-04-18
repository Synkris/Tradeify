using Core.Config;
using Core.DB;
using Core.Models;
using Logic.IHelpers;
using Logic.Services;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Helpers
{
	public interface IBaseHelper
	{
		void LogCritical(string message);
		void LogError(string error);
		void LogInformation(string error);
		void LogWarning(string error);
	}

	public class BaseHelper : IBaseHelper
	{
		//Logging Methods
		public void LogCritical(string message)
		{
			Log.Logger.Fatal(message);
		}
		public void LogError(string error)
		{
			Log.Logger.Error(error);
		}
		public void LogInformation(string error)
		{
			Log.Logger.Information(error);
		}
		public void LogWarning(string error)
		{
			Log.Logger.Warning(error);
		}

	}
}
