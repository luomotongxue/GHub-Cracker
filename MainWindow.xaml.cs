using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.IO;
using HandyControl.Controls;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GHub_Cracker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window
    {
        public class ResponseModel
        {
            public List<Customer> Data { get; set; }
        }

        public class Customer
        {
            public Dictionary<string, string> Metadata { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();

            var buttonToBringToFront = growlParent;
            Grid parentGrid = (Grid)buttonToBringToFront.Parent;

            int newZIndex = int.MaxValue; 
 
            Grid.SetZIndex(buttonToBringToFront, newZIndex);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            long time = 0;

            try
            {
                time = Int64.Parse(timeTxtBox.Text);
            }
            catch
            {
                Growl.Info("请输入有效的数字");

                return;
            }

            string jsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                "\\GreenHub\\config.json";
            string newMinutesValue = time.ToString();
            string jsContent;
            // 读取文件内容  
            try
            {
                jsContent = File.ReadAllText(jsFilePath);
            }
            catch
            {
                Growl.Error("无法打开文件 - " + jsFilePath);

                return;
            }

            // 使用正则表达式找到要替换的minutes值  
            // 假设minutes的值是一个数字，并且你想要替换整个数字（包括前面的逗号和空格）  
            string pattern = @"""minutes"":\s*\d+"; // 匹配 "minutes": 后面的空格和任意数字  
            string replacement = $"\"minutes\": {newMinutesValue}"; // 创建替换字符串，注意格式要保持一致  

            // 执行替换  
            string newJsContent = Regex.Replace(jsContent, pattern, replacement);

            // 写回文件  
            File.WriteAllText(jsFilePath, newJsContent);

            Growl.Success("完成！刷新节点后生效");
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // 强制使用 TLS 1.2
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            var client = new HttpClient();

            // 设置请求头，使用从 Python 获取的相同 Authorization 信息
            string authHeader = "cmtfbGl2ZV81MUw5c3JLQ29GamtETjR4UGtvdEs2V0dHUHFmd2tnd3RFNkkxcTFURTlrdktzZ0s3SlQ5Mk5oaUFHeGpKeDQ0ejdHZnBzU1hZNmtpTVkyTTFWWkhFajJZVjAwbjRPS3pUSlg6";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            // 发起 GET 请求
            var response = await client.GetAsync("https://api.stripe.com/v1/customers");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(jsonString);

                // 提取 license_code
                var licenseCodes = new List<string>();
                foreach (var customer in result["data"])
                {
                    licenseCodes.Add(customer["metadata"]["license_code"].ToString());
                }

                // 打印结果
                codesTxtBox.Text = string.Join("\r\n", licenseCodes);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                codesTxtBox.Text = $"Error: {response.StatusCode} - {response.ReasonPhrase}\r\nResponse Content: {errorContent}";
            }
        }
    }
}
