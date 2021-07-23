using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Common.Helper
{
    /// <summary>
    ///     配置帮助类
    /// </summary>
    public class ConfigHelper
    {
        /* 使用热更新
        var uploadConfig = new ConfigHelper().Load("uploadconfig", _env.EnvironmentName, true);
        services.Configure<UploadConfig>(uploadConfig);

        private readonly UploadConfig _uploadConfig;
        public ImgController(IOptionsMonitor<UploadConfig> uploadConfig)
        {
            _uploadConfig = uploadConfig.CurrentValue;
        }
        */

        /// <summary>
        ///     加载配置文件
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="environmentName">环境名称</param>
        /// <param name="reloadOnChange">自动更新</param>
        /// <returns></returns>
        public IConfiguration Load(string fileName, string environmentName = "", bool reloadOnChange = false)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory);
            if (!Directory.Exists(filePath))
            {
                return null;
            }

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(filePath)
                .AddJsonFile(fileName.ToLower() + ".json", true, reloadOnChange);

            if (!string.IsNullOrEmpty(environmentName))
            {
                builder.AddJsonFile(fileName.ToLower() + "." + environmentName + ".json", true, reloadOnChange);
            }

            return builder.Build();
        }

        /// <summary>
        ///     获得配置信息
        /// </summary>
        /// <typeparam name="T">配置信息</typeparam>
        /// <param name="fileName">文件名称</param>
        /// <param name="environmentName">环境名称</param>
        /// <param name="reloadOnChange">自动更新</param>
        /// <returns></returns>
        public T Get<T>(string fileName, string environmentName = "", bool reloadOnChange = false)
        {
            IConfiguration configuration = Load(fileName, environmentName, reloadOnChange);
            if (configuration == null)
            {
                return default;
            }

            return configuration.Get<T>();
        }

        /// <summary>
        ///     绑定实例配置信息
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="instance">实例配置</param>
        /// <param name="environmentName">环境名称</param>
        /// <param name="reloadOnChange">自动更新</param>
        public void Bind(string fileName, object instance, string environmentName = "", bool reloadOnChange = false)
        {
            IConfiguration configuration = Load(fileName, environmentName, reloadOnChange);
            if (configuration == null || instance == null)
            {
                return;
            }

            configuration.Bind(instance);
        }
    }
}