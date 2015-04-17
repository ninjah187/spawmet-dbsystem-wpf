﻿#pragma checksum "..\..\MachinesWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "614F49B06A156416B7255CFE7E522D7C"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using SpawmetDatabase.Model;
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
    /// MachinesWindow
    /// </summary>
    public partial class MachinesWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 14 "..\..\MachinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem PartsMenuItem;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\MachinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem ConnectMenuItem;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\MachinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid MainDataGrid;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\MachinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem AddContextMenuItem;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\MachinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem DeleteContextMenuItem;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\MachinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel DetailsStackPanel;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\MachinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock IdTextBlock;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\MachinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock NameTextBlock;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\MachinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock PriceTextBlock;
        
        #line default
        #line hidden
        
        
        #line 58 "..\..\MachinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock StandardPartsTextBlock;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\MachinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid StandardPartSetDataGrid;
        
        #line default
        #line hidden
        
        
        #line 74 "..\..\MachinesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox OrdersListBox;
        
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
            System.Uri resourceLocater = new System.Uri("/SpawmetDatabaseWPF;component/machineswindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\MachinesWindow.xaml"
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
            
            #line 14 "..\..\MachinesWindow.xaml"
            this.PartsMenuItem.Click += new System.Windows.RoutedEventHandler(this.PartsMenuItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 2:
            this.ConnectMenuItem = ((System.Windows.Controls.MenuItem)(target));
            
            #line 17 "..\..\MachinesWindow.xaml"
            this.ConnectMenuItem.Click += new System.Windows.RoutedEventHandler(this.ConnectMenuItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 3:
            this.MainDataGrid = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 4:
            this.AddContextMenuItem = ((System.Windows.Controls.MenuItem)(target));
            
            #line 29 "..\..\MachinesWindow.xaml"
            this.AddContextMenuItem.Click += new System.Windows.RoutedEventHandler(this.AddContextMenuItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.DeleteContextMenuItem = ((System.Windows.Controls.MenuItem)(target));
            
            #line 30 "..\..\MachinesWindow.xaml"
            this.DeleteContextMenuItem.Click += new System.Windows.RoutedEventHandler(this.DeleteContextMenuItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 31 "..\..\MachinesWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.SaveContextMenuItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 32 "..\..\MachinesWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.RefreshContextMenuItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 8:
            this.DetailsStackPanel = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 9:
            this.IdTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 10:
            this.NameTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 11:
            this.PriceTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 12:
            this.StandardPartsTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 13:
            this.StandardPartSetDataGrid = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 14:
            
            #line 63 "..\..\MachinesWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.AddPartItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 15:
            
            #line 64 "..\..\MachinesWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.DeletePartItem_OnClick);
            
            #line default
            #line hidden
            return;
            case 16:
            this.OrdersListBox = ((System.Windows.Controls.ListBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
