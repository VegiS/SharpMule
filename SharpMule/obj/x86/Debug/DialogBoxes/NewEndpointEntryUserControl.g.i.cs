﻿#pragma checksum "..\..\..\..\DialogBoxes\NewEndpointEntryUserControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "4C32456E260A91E6362B0F99052D8B4D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
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


namespace SharpMule.DialogBoxes {
    
    
    /// <summary>
    /// NewEndpointEntryUserControl
    /// </summary>
    public partial class NewEndpointEntryUserControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 7 "..\..\..\..\DialogBoxes\NewEndpointEntryUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal SharpMule.DialogBoxes.NewEndpointEntryUserControl UCAddNewEndPoint;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\..\..\DialogBoxes\NewEndpointEntryUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnAddEP;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\..\..\DialogBoxes\NewEndpointEntryUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtEndpoint;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\..\..\DialogBoxes\NewEndpointEntryUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtPort;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\..\DialogBoxes\NewEndpointEntryUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label label2;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\..\DialogBoxes\NewEndpointEntryUserControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label label3;
        
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
            System.Uri resourceLocater = new System.Uri("/SharpMule;component/dialogboxes/newendpointentryusercontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\DialogBoxes\NewEndpointEntryUserControl.xaml"
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
            this.UCAddNewEndPoint = ((SharpMule.DialogBoxes.NewEndpointEntryUserControl)(target));
            return;
            case 2:
            this.btnAddEP = ((System.Windows.Controls.Button)(target));
            
            #line 9 "..\..\..\..\DialogBoxes\NewEndpointEntryUserControl.xaml"
            this.btnAddEP.Click += new System.Windows.RoutedEventHandler(this.btnAddEP_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.txtEndpoint = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.txtPort = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.label2 = ((System.Windows.Controls.Label)(target));
            return;
            case 6:
            this.label3 = ((System.Windows.Controls.Label)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

