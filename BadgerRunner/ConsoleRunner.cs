using Badger.Core;
using Badger.Runner.Helpers;
using Badger.Runner.Presenters;
using Badger.Runner.Views;
using CommandLine;
using System;
using System.Windows.Forms;
using Autofac;
using CommandLine.Text;

namespace Badger.Runner
{
    class ConsoleRunner
    {
        [STAThread]
        static int Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                string resourcePath = String.IsNullOrEmpty(options.ResourcePath) ? null : options.ResourcePath;
                string outPath = String.IsNullOrEmpty(options.OutputPath) ? Environment.CurrentDirectory : options.OutputPath;
                string tags = String.IsNullOrEmpty(options.Tags) ? null : options.Tags;
                string excludeTags = String.IsNullOrEmpty(options.ExcludeTags) ? null : options.ExcludeTags;

                if (!String.IsNullOrEmpty(options.TestPath))
                {
                    string testPath = options.TestPath;
                    var fileService = new FileService();
                    var runner = new TestRunner(new TestService(fileService), fileService);
                    bool result = runner.RunTests(testPath, outPath, resourcePath, tags, excludeTags);
                    return result ? 0 : 1;

                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    ContainerBuilder builder = new ContainerBuilder();
                    builder.RegisterType<BadgerGui>().As<Interfaces.IBadgerGui>();
                    builder.RegisterType<BadgerGuiPresenter>().As<Interfaces.IBadgerGuiPresenter>();
                    builder.RegisterType<BadgerTestEditor>().As<Interfaces.ITestEditorView>();
                    builder.RegisterType<BadgerTestEditorPresenter>().As<Interfaces.ITestEditorPresenter>();
                    builder.RegisterType<ReportView>().As<Interfaces.IReportView>();
                    builder.RegisterType<ReportViewPresenter>().As<Interfaces.IReportViewPresenter>();
                    builder.RegisterType<FileService>().As<Core.Interfaces.IFileService>();
                    builder.RegisterType<FolderBrowser>().As<Interfaces.IFolderBrowser>();
                    builder.RegisterType<FileBrowser>().As<Interfaces.IFileBrowser>();
                    builder.RegisterType<MessageBoxDialog>().As<Interfaces.IMessageBoxService>();

                    var container = builder.Build();

                    using (var scope = container.BeginLifetimeScope())
                    {
                        var presenter = scope.Resolve<Interfaces.IBadgerGuiPresenter>();
                        presenter.ResourcePath = resourcePath;
                        presenter.ShowView();
                    }
                    
                }
            }
            return 0;

        }
    }

    class Options
    {
        [Option('p', "path", HelpText = "Path to test or test folder")]
        public string TestPath { get; set; }

        [Option('o', "output", HelpText = "Path for output")]
        public string OutputPath { get; set; }

        [Option('r', "resource", HelpText = "Path to resoucre file")]
        public string ResourcePath { get; set; }

        [Option('t', "tags", HelpText = "Tags")]
        public string Tags { get; set; }

        [Option('e', "exclude", HelpText = "Tags to exclude")]
        public string ExcludeTags { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

}
