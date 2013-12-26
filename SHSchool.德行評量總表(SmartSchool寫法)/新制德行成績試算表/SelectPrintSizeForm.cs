using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.Xml;
using FISCA.Presentation.Controls;
using FISCA.DSAUtil;

namespace �w�榨�Z�պ��
{
    public partial class SelectPrintSizeForm : BaseForm
    {
        string _ConfigPrint;

        Campus.Configuration.ConfigData cd;

        /// <summary>
        /// �ǤJ�]�w�ɦW�ٻP�]�w�ɰѼ�
        /// </summary>
        /// <param name="ConfigName">�]�w�ɦW��</param>
        public SelectPrintSizeForm(string ConfigPrint)
        {
            InitializeComponent();

            _ConfigPrint = ConfigPrint;

            cd = Campus.Configuration.Config.User[_ConfigPrint];
            string config = cd["�ȱi�]�w"];
            int x = 0;
            if (!string.IsNullOrEmpty(config))
            {
                //�C�L��T
                int.TryParse(config, out x);
            }
            comboBoxEx1.SelectedIndex = x;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            int x = comboBoxEx1.SelectedIndex;
            cd["�ȱi�]�w"] = x.ToString();
            cd.Save();
            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}