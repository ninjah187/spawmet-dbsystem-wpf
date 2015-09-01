﻿#pragma checksum "..\..\ArchiveWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "CDD78C170415EF2E7E76805E45027C5D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SpawmetDatabaseWPF {
    
    
    /// <summary>
    /// ArchiveWindow
    /// </summary>
    public partial class ArchiveWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 19 "..\..\ArchiveWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem PartsMenuItem;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\ArchiveWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem OrdersMenuItem;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\ArchiveWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem ClientsMenuItem;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\ArchiveWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem ConnectMenuItem;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\ArchiveWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem SearchMenuItem;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\ArchiveWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem AddMachinesFromDirectoryMenuItem;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\ArchiveWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid MainDataGrid;
        
        #line default
        #line hidden
        
        
        #line 103 "..\..\ArchiveWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid StandardPartSetDataGrid;
        
        #line default
        #line hidden
        
        
        #line 118 "..\..\ArchiveWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid AdditionalPartSetDataGrid;
        
        #line default
        #line hidden
        
        
        #line 136 "..\..\ArchiveWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox ModulesListBox;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SpawmetDatabaseWPF;component/archivewindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\ArchiveWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.PartsMenuItem = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 2:
            this.OrdersMenuItem = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 3:
            this.ClientsMenuItem = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 4:
            this.ConnectMenuItem = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 5:
            this.SearchMenuItem = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 6:
            this.AddMachinesFromDirectoryMenuItem = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 7:
            this.MainDataGrid = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 8:
            this.StandardPartSetDataGrid = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 9:
            this.AdditionalPartSetDataGrid = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 10:
            this.ModulesListBox = ((System.Windows.Controls.ListBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
