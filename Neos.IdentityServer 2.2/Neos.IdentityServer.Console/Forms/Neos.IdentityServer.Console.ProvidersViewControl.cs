﻿//******************************************************************************************************************************************************************************************//
// Copyright (c) 2017 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
// https://adfsmfa.codeplex.com                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ManagementConsole;
using Neos.IdentityServer.MultiFactor.Administration;
using System.Threading;
using Neos.IdentityServer.MultiFactor;
using System.DirectoryServices;
using Microsoft.ManagementConsole.Advanced;
using Neos.IdentityServer.Console.Controls;

namespace Neos.IdentityServer.Console
{
    public partial class ProvidersViewControl : UserControl, IFormViewControl, IMMCNotificationData
    {
        private Control oldParent;
        private ServiceProvidersFormView _frm = null;
        private bool _isnotifsenabled = true;
        private MFAProvidersValidationControl _ctrl;
        private List<MFAProvidersControl> _lst = new List<MFAProvidersControl>();

        public ProvidersViewControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize method 
        /// </summary>
        public void Initialize(FormView view)
        {
            FormView = (ServiceProvidersFormView)view;
            OnInitialize();
        }

        /// <summary>
        /// OnInitialize method
        /// </summary>
        protected virtual void OnInitialize()
        {
            this.SuspendLayout();
            this.HorizontalScroll.Enabled = false;
            this.HorizontalScroll.Visible = false;
            try
            {
                _lst.Clear();
                MFAProvidersControl tmp = null;
                int i = 2;
                IExternalProvider totp = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Code);
                if (totp != null)
                {
                    tmp = new MFAProvidersControl(this, this.SnapIn, totp);
                    _lst.Add(tmp);
                    this.tableLayoutPanel.Controls.Add(tmp, 0, i);
                    i++;
                }
                IExternalProvider email = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Email);
                if(email != null)
                {
                    tmp = new MFAProvidersControl(this, this.SnapIn, email);
                    _lst.Add(tmp);
                    this.tableLayoutPanel.Controls.Add(tmp, 0, i);
                    i++;
                }
                IExternalProvider phone = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.External);
                if (phone != null)
                {
                    tmp = new MFAProvidersControl(this, this.SnapIn, phone);
                    _lst.Add(tmp);
                    this.tableLayoutPanel.Controls.Add(tmp, 0, i);
                    i++;
                }
                IExternalProvider azure = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Azure);
                if (azure != null)
                {
                    tmp = new MFAProvidersControl(this, this.SnapIn, azure);
                    _lst.Add(tmp);
                    this.tableLayoutPanel.Controls.Add(tmp, 0, i);
                    i++;
                }
                ControlInstance = new MFAProvidersValidationControl(this, this.SnapIn);
                this.tableLayoutPanel.Controls.Add(ControlInstance, 0, i); 
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        #region Properties
        /// <summary>
        /// FormView property implementation
        /// </summary>
        protected ServiceProvidersFormView FormView
        {
            get { return _frm; }
            private set { _frm = value; }
        }

        /// <summary>
        /// ControlInstance property implmentation
        /// </summary>
        protected MFAProvidersValidationControl ControlInstance
        {
            get { return _ctrl; }
            private set { _ctrl = value; }
        }

        /// <summary>
        /// SnapIn method implementation
        /// </summary>
        protected NamespaceSnapInBase SnapIn
        {
            get { return this.FormView.ScopeNode.SnapIn; }
        }

        /// <summary>
        /// ScopeNode method implementation
        /// </summary>
        protected ServiceProvidersScopeNode ScopeNode
        {
            get { return this.FormView.ScopeNode as ServiceProvidersScopeNode; }
        }

        /// <summary>
        /// OnParentChanged method override
        /// </summary>
        protected override void OnParentChanged(EventArgs e)
        {
            if (Parent != null)
            {
                if (!DesignMode)
                    Size = Parent.ClientSize;
                Parent.SizeChanged += Parent_SizeChanged;
            }
            if (oldParent != null)
            {
                oldParent.SizeChanged -= Parent_SizeChanged;
            }
            oldParent = Parent;
            base.OnParentChanged(e);
        }

        /// <summary>
        /// Parent_SizeChanged event
        /// </summary>
        private void Parent_SizeChanged(object sender, EventArgs e)
        {
            if (!DesignMode)
                Size = Parent.ClientSize;
        }
        #endregion

        /// <summary>
        /// RefreshData method implementation
        /// </summary>
        internal void RefreshData()
        {
            this.SuspendLayout();
            this.Cursor = Cursors.WaitCursor;
            _isnotifsenabled = false;
            try
            {
                ComponentResourceManager resources = new ComponentResourceManager(typeof(ProvidersViewControl));
                this.label1.Text = resources.GetString("label1.Text");
                this.label2.Text = resources.GetString("label2.Text");
                this.label3.Text = resources.GetString("label3.Text");

                ManagementService.ADFSManager.ReadConfiguration(null);
                ((IMMCRefreshData)ControlInstance).DoRefreshData();
                foreach (MFAProvidersControl ct in _lst)
                {
                    ((IMMCRefreshData)ct).DoRefreshData();
                }
            }
            finally
            {
                _isnotifsenabled = true;
                this.Cursor = Cursors.Default;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// CancelData method implementation
        /// </summary>
        internal void CancelData()
        {
            try
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                ComponentResourceManager resources = new ComponentResourceManager(typeof(ProvidersViewControl));
                messageBoxParameters.Text = resources.GetString("MFAVALIDSAVE");
                messageBoxParameters.Buttons = MessageBoxButtons.YesNo;
                messageBoxParameters.Icon = MessageBoxIcon.Question;
                if (this.SnapIn.Console.ShowDialog(messageBoxParameters) == DialogResult.Yes)
                   RefreshData();
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this.SnapIn.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SaveData method implementation
        /// </summary>
        internal void SaveData()
        {
            if (this.ValidateChildren())
            {
                this.Cursor = Cursors.WaitCursor;
                _isnotifsenabled = false;
                try
                {
                    ManagementService.ADFSManager.WriteConfiguration(null);
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Default;
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                    messageBoxParameters.Text = ex.Message;
                    messageBoxParameters.Buttons = MessageBoxButtons.OK;
                    messageBoxParameters.Icon = MessageBoxIcon.Error;
                    this.SnapIn.Console.ShowDialog(messageBoxParameters);
                }
                finally
                {
                    _isnotifsenabled = true;
                    this.Cursor = Cursors.Default;
                }
            }
        }

        /// <summary>
        /// IsNotifsEnabled method implmentation
        /// </summary>
        public bool IsNotifsEnabled()
        {
            return _isnotifsenabled;
        }
    }
}
