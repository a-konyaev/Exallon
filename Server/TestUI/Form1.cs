using System;
using System.ServiceModel;
using System.Windows.Forms;
using Exallon.Data;

namespace Exallon.TestUI
{
    public partial class Form1 : Form
    {
        private ChannelFactory<IDataService> _proxy;
        private IDataService _service;
        private String _sessionId;

        public Form1()
        {
            InitializeComponent();
            OpenChannel();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OpenChannel();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseChannel();
        }

        private void OpenChannel()
        {
            try
            {
                _proxy = new ChannelFactory<IDataService>("Exallon");
                _proxy.Open();
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Не удалось прокси-класс для взаимодействия с сервером",
                    "Ошибка создания прокси-класса",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _proxy = null;
                return;
            }

            _service = _proxy.CreateChannel();
        }

        private void CloseChannel()
        {
            if (_proxy == null || _proxy.State != CommunicationState.Opened)
                return;

            try
            {
                _proxy.Close(TimeSpan.FromSeconds(1));
            }
            finally
            {
                _proxy = null;
                _service = null;
                _sessionId = null;
            }
        }

        private void SafeRemoteCall(Action action)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show(msg, "Ошибка при обращении к серверу", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                tabControl1.Enabled = false;
            }

            Cursor = Cursors.Default;
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            SessionResponse resp = null;
            SafeRemoteCall(() => { resp = _service.Authorize(textBoxLogin.Text, textBoxPassword.Text); });

            if (resp == null)
                return;

            if (resp.Failed)
            {
                MessageBox.Show(
                    resp.ErrorDescription,
                    "Ошибка авторизации",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                tabControl1.Enabled = false;
                return;
            }

            _sessionId = resp.SessionId;
            tabControl1.Enabled = true;
        }

        private void buttonGetCatalogs_Click(object sender, EventArgs e)
        {
            GetDataResponse resp = null;
            SafeRemoteCall(() => { resp = _service.GetCatalogs(_sessionId); });

            if (resp == null)
                return;

            if (resp.Failed)
            {
                MessageBox.Show(
                    resp.ErrorDescription,
                    "Ошибка получения списка справочников",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            dataGridViewResults.Columns.Clear();
            dataGridViewResults.Columns.Add("Name", "Имя справочника");
            dataGridViewResults.Columns.Add("Presentation", "Имя справочника для отображения");

            dataGridViewResults.Columns[0].Width = 300;
            dataGridViewResults.Columns[1].Width = 300;

            dataGridViewResults.Rows.Clear();
            foreach (var catalogDescriptor in resp.Data)
            {
                dataGridViewResults.Rows.Add
                    (
                        new object[] {catalogDescriptor.Id, catalogDescriptor.Name}
                    );
            }
        }

        private void buttonGetCatalogItems_Click(object sender, EventArgs e)
        {
            string parentId = textBoxParentId.Text;

            int indexFrom;
            int indexTo;

            if (!int.TryParse(textBoxItemNumFrom.Text, out indexFrom))
                indexFrom = -1;

            if (!int.TryParse(textBoxItemNumTo.Text, out indexTo))
                indexTo = -1;

            GetDataResponse resp = null;
            SafeRemoteCall(
                () =>
                    {
                        if (checkBoxGetAll.Checked)
                        {
                            resp = _service.GetAllCatalogItems(_sessionId, textBoxCatalogName.Text);
                        }
                        else
                        {
                            resp = _service.GetCatalogItems(
                                _sessionId,
                                textBoxCatalogName.Text,
                                indexFrom,
                                indexTo,
                                parentId);
                        }
                    });

            if (resp == null)
                return;

            if (resp.Failed)
            {
                MessageBox.Show(
                    resp.ErrorDescription,
                    "Ошибка получения элементов справочника",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            dataGridViewResults.Columns.Clear();
            dataGridViewResults.Columns.Add("Name", "Идентификатор");
            dataGridViewResults.Columns.Add("Presentation", "Название");

            dataGridViewResults.Columns[0].Width = 300;
            dataGridViewResults.Columns[1].Width = 300;

            dataGridViewResults.Rows.Clear();
            foreach (var item in resp.Data)
            {
                dataGridViewResults.Rows.Add
                    (
                        new object[] { item.Id, item.Name }
                    );
            }
        }

        private void dataGridViewResults_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (tabControl1.SelectedIndex != 1)
                return;

            var itemId = (string) dataGridViewResults.Rows[e.RowIndex].Cells[0].Value;

            GetDataResponse resp = null;
            SafeRemoteCall(
                () =>
                    {
                        resp = _service.GetCatalogItemDetails(_sessionId, textBoxCatalogName.Text, itemId);
                    });

            if (resp == null)
                return;

            if (resp.Failed)
            {
                MessageBox.Show(
                    resp.ErrorDescription,
                    "Ошибка получения детальной инф-ции элемента справочника",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            dataGridViewResults.Columns.Clear();
            dataGridViewResults.Columns.Add("PropName", "Наименование");
            dataGridViewResults.Columns.Add("PropValue", "Значение");

            dataGridViewResults.Columns[0].Width = 300;
            dataGridViewResults.Columns[1].Width = 300;

            dataGridViewResults.Rows.Clear();
            foreach (var item in resp.Data)
            {
                dataGridViewResults.Rows.Add
                    (
                        new object[] { item.Name, item.Value }
                    );
            }
        }

        private void buttonGetDocuments_Click(object sender, EventArgs e)
        {
            GetDataResponse resp = null;
            SafeRemoteCall(() => { resp = _service.GetDocuments(_sessionId); });

            if (resp == null)
                return;

            if (resp.Failed)
            {
                MessageBox.Show(
                    resp.ErrorDescription,
                    "Ошибка получения списка документов",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            dataGridViewResults.Columns.Clear();
            dataGridViewResults.Columns.Add("Name", "Имя документа");
            dataGridViewResults.Columns.Add("Presentation", "Имя документа для отображения");

            dataGridViewResults.Columns[0].Width = 300;
            dataGridViewResults.Columns[1].Width = 300;

            dataGridViewResults.Rows.Clear();
            foreach (var item in resp.Data)
            {
                dataGridViewResults.Rows.Add
                    (
                        new object[] { item.Id, item.Name }
                    );
            }
        }
    }
}
