using Melbeez.Business.Managers.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melbeez.Services
{
    public class AlertSMSCountService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;

        public AlertSMSCountService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DailySMSCountService, null, getJobRunDelay(), new TimeSpan(24, 0, 0));
            return Task.CompletedTask;
        }
        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        private void DailySMSCountService(object? state)
        {
            try
            {
                WriteLog("SMS count service started..");
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _smsCountNotify = scope.ServiceProvider.GetService<ISMSTransactionLogManager>();
                    _smsCountNotify.GetDailySentSMSCount();
                }
                WriteLog("SMS count service ended..");
            }
            catch (Exception ex)
            {
                WriteLog("Error in SMSCountService : " + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            
        }
        private TimeSpan getScheduledParsedTime()
        {
            IConfiguration _configuration;
            using (var scope = _serviceProvider.CreateScope())
            {
                _configuration = scope.ServiceProvider.GetService<IConfiguration>();
            }
            string[] formats = { @"hh\:mm\:ss", "hh\\:mm" };
            string jobStartTime = _configuration["SmsCountSerivceRunTime"];
            TimeSpan.TryParseExact(jobStartTime, formats, CultureInfo.InvariantCulture, out TimeSpan ScheduledTimespan);
            WriteLog("Scheduled time is : " + ScheduledTimespan);
            return ScheduledTimespan;
        }
        private TimeSpan getJobRunDelay()
        {
            WriteLog("Service Started at " + DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm"));
            TimeSpan scheduledParsedTime = getScheduledParsedTime();
            TimeSpan curentTimeOftheDay = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString("hh\\:mm"));
            TimeSpan delayTime = scheduledParsedTime >= curentTimeOftheDay
                                 ? scheduledParsedTime - curentTimeOftheDay
                                 : new TimeSpan(24, 0, 0) - curentTimeOftheDay + scheduledParsedTime;
            WriteLog("Delay time is : " + delayTime);
            return delayTime;
        }
        public static void WriteLog(string strLog)
        {
            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            string logFilePath = "C:\\Applications\\Melbeez\\Logs\\";
            logFilePath = logFilePath + "Log-sms-count-service" + System.DateTime.Today.ToString("MM-dd-yyyy") + "." + "txt";
            logFileInfo = new FileInfo(logFilePath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            log = new StreamWriter(fileStream);
            log.WriteLine(strLog);
            log.Close();
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
