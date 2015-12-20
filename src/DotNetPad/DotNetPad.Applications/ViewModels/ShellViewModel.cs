﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Waf.Applications;
using System.Windows.Input;
using Waf.DotNetPad.Applications.DataModels;
using Waf.DotNetPad.Applications.Properties;
using Waf.DotNetPad.Applications.Services;
using Waf.DotNetPad.Applications.Views;

namespace Waf.DotNetPad.Applications.ViewModels
{
    [Export]
    public class ShellViewModel : ViewModel<IShellView>
    {
        private readonly IShellService shellService;
        private readonly IFileService fileService;
        private readonly ICSharpSampleService csharpSampleService;
        private readonly IVisualBasicSampleService visualBasicSampleService;
        private readonly AppSettings settings;
        private readonly DelegateCommand garbageCollectorCommand;
        private object errorListView;
        private object outputView;
        private ICommand startCommand;
        private ICommand stopCommand;
        private bool isScriptRunning;
        private ICommand infoCommand;
        private object currentStatusView;
        private IReadOnlyList<DocumentDataModel> documentDataModels;
        private DocumentDataModel activeDocumentDataModel;
        private string statusText;
        

        [ImportingConstructor]
        public ShellViewModel(IShellView view, IShellService shellService, IFileService fileService, ICSharpSampleService csharpSampleService, 
            IVisualBasicSampleService visualBasicSampleService)
            : base(view)
        {
            this.shellService = shellService;
            this.fileService = fileService;
            this.csharpSampleService = csharpSampleService;
            this.visualBasicSampleService = visualBasicSampleService;
            this.settings = shellService.Settings;
            this.garbageCollectorCommand = new DelegateCommand(() => GC.Collect());
            this.statusText = Resources.Ready;
            
            PropertyChangedEventManager.AddHandler(fileService, FileServicePropertyChanged, "");
            view.Closed += ViewClosed;

            // Restore the window size when the values are valid.
            if (settings.Left >= 0 && settings.Top >= 0 && settings.Width > 0 && settings.Height > 0
                && settings.Left + settings.Width <= view.VirtualScreenWidth
                && settings.Top + settings.Height <= view.VirtualScreenHeight)
            {
                view.Left = settings.Left;
                view.Top = settings.Top;
                view.Height = settings.Height;
                view.Width = settings.Width;
                view.BottomPanesHeight = settings.BottomPanesHeight;
            }
            view.IsMaximized = settings.IsMaximized;
        }

        
        public string Title { get { return ApplicationInfo.ProductName; } }

        public IShellService ShellService { get { return shellService; } }

        public IFileService FileService { get { return fileService; } }

        public ICSharpSampleService CSharpSampleService { get { return csharpSampleService; } }

        public IVisualBasicSampleService VisualBasicSampleService { get { return visualBasicSampleService; } }

        public object ErrorListView
        {
            get { return errorListView; }
            set { SetProperty(ref errorListView, value); }
        }

        public object OutputView
        {
            get { return outputView; }
            set { SetProperty(ref outputView, value); }
        }

        public ICommand StartCommand
        {
            get { return startCommand; }
            set { SetProperty(ref startCommand, value); }
        }

        public ICommand StopCommand
        {
            get { return stopCommand; }
            set { SetProperty(ref stopCommand, value); }
        }

        public bool IsScriptRunning
        {
            get { return isScriptRunning; }
            set { SetProperty(ref isScriptRunning, value); }
        }

        public ICommand InfoCommand
        {
            get { return infoCommand; }
            set { SetProperty(ref infoCommand, value); }
        }

        public ICommand GarbageCollectorCommand { get { return garbageCollectorCommand; } }

        public object CurrentStatusView
        {
            get { return currentStatusView; }
            private set { SetProperty(ref currentStatusView, value); }
        }
        
        public bool IsErrorListViewVisible
        {
            get { return CurrentStatusView == ErrorListView; }
            set { if (value) { CurrentStatusView = ErrorListView; } }
        }

        public bool IsOutputViewVisible
        {
            get { return CurrentStatusView == OutputView; }
            set { if (value) { CurrentStatusView = OutputView; } }
        }

        public IReadOnlyList<DocumentDataModel> DocumentDataModels
        {
            get { return documentDataModels; }
            set { SetProperty(ref documentDataModels, value); }
        }

        public DocumentDataModel ActiveDocumentDataModel
        {
            get { return activeDocumentDataModel; }
            set 
            { 
                if (SetProperty(ref activeDocumentDataModel, value) && value != null)
                {
                    FileService.ActiveDocumentFile = value.DocumentFile;
                }
            }
        }

        public string StatusText
        {
            get { return statusText; }
            set 
            { 
                if (statusText != value)
                {
                    statusText = value ?? Resources.Ready;
                    RaisePropertyChanged();
                }
            }
        }


        public void Show()
        {
            ViewCore.Show();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == "CurrentStatusView")
            {
                RaisePropertyChanged("IsErrorListViewVisible");
                RaisePropertyChanged("IsOutputViewVisible");
            }
        }

        private void FileServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveDocumentFile")
            {
                ActiveDocumentDataModel = DocumentDataModels.FirstOrDefault(x => x.DocumentFile == fileService.ActiveDocumentFile);
            }
        }

        private void ViewClosed(object sender, EventArgs e)
        {
            settings.Left = ViewCore.Left;
            settings.Top = ViewCore.Top;
            settings.Height = ViewCore.Height;
            settings.Width = ViewCore.Width;
            settings.IsMaximized = ViewCore.IsMaximized;
            settings.BottomPanesHeight = ViewCore.BottomPanesHeight;
        }
    }
}
