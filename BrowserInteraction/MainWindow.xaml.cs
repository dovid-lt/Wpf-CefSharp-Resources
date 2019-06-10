using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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

namespace BrowserInteraction
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        CefSharp.Wpf.ChromiumWebBrowser browser;

        public MainWindow()
        {
            CefSettings settings = new CefSettings();
            settings.RegisterScheme(new CefCustomScheme()
            {
                SchemeName = ResourceSchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new ResourceSchemeHandlerFactory()
            });
            Cef.Initialize(settings);
            browser = new ChromiumWebBrowser();
            InitializeComponent();




            container.Children.Add(browser);
            browser.Address = "resource://client-app/index.html ";


        }



    }


    class ResourceSchemeHandlerFactory : ISchemeHandlerFactory
    {

        public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
        {
            return new ResourceSchemeHandler();
        }

        public static string SchemeName { get { return "resource"; } }
    }

    public class ResourceSchemeHandler : ResourceHandler
    {
        public override bool ProcessRequestAsync(IRequest request, ICallback callback)
        {
            Task.Run(() =>
            {
                using (callback)
                {
                    Uri u = new Uri(request.Url);
                    String file = u.Authority + u.AbsolutePath;

                    Assembly ass = Assembly.GetExecutingAssembly();
                    String resourcePath = ass.GetName().Name + "." + file.Replace("/", ".").Replace("-", "_");

                    if (ass.GetManifestResourceInfo(resourcePath) != null)
                    {
                        Stream = ass.GetManifestResourceStream(resourcePath);
                        switch (System.IO.Path.GetExtension(file))
                        {
                            case ".html":
                                MimeType = "text/html";
                                break;
                            case ".js":
                                MimeType = "text/javascript";
                                break;
                            case ".png":
                                MimeType = "image/png";
                                break;
                            case ".appcache":
                            case ".manifest":
                                MimeType = "text/cache-manifest";
                                break;
                            default:
                                MimeType = "application/octet-stream";
                                break;
                        }

                        ResponseLength = Stream.Length;
                        StatusCode = (int)HttpStatusCode.OK;

                        callback.Continue();
                    }
                }
            });
            return true;
        }
    }
}
