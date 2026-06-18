using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Tambahkan ini
using System.Data.SqlClient;

namespace CRUDMahasiswaADO
{
    public partial class Form2 : Form
    {
        static string connectionString =
            "Data Source=IZAYAAA\\IZA;Initial Catalog=DBAkademikADO;Persist Security Info=True;User ID=sa;Password=Kuliah01;TrustServerCertificate=True";

        SqlConnection conn = new SqlConnection(connectionString);
        SqlDataAdapter da;
        DataTable dtMahasiswa;
        DataTable dtProdi;

        List<Data> listMahasiswa = new List<Data>();

        string prodi { get; set; }
        DateTime tglMasuk { get; set; }

        public Form2()
        {
            InitializeComponent();
            button2.Click += button2_Click;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "yyyy";
            dateTimePicker1.ShowUpDown = true;
            dateTimePicker1.MinDate = new DateTime(2000, 1, 1);
            dateTimePicker1.MaxDate = DateTime.Now;

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            button2.Enabled = false;

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                SqlCommand cmd = new SqlCommand(
                    "select namaprodi from programstudi",
                    conn);

                cmd.CommandType = CommandType.Text;

                dtProdi = new DataTable();
                da = new SqlDataAdapter(cmd);

                da.Fill(dtProdi);

                comboBox1.DataSource = dtProdi;
                comboBox1.DisplayMember = "namaprodi";
                comboBox1.ValueMember = "namaprodi";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data : " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                SqlCommand cmd = new SqlCommand("sp_Report", conn);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@inProdi", SqlDbType.VarChar, 50)
                    .Value = comboBox1.SelectedValue;

                cmd.Parameters.Add("@inTglMsuk", SqlDbType.VarChar, 4)
                    .Value = dateTimePicker1.Value.Year.ToString();

                da = new SqlDataAdapter(cmd);

                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                dataGridView1.DataSource = dtMahasiswa;

                if (dtMahasiswa.Rows.Count > 0)
                {
                    button2.Enabled = true;
                }
                else
                {
                    button2.Enabled = false;
                    MessageBox.Show("Data tidak ditemukan");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data : " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Report frm = new Report(
                comboBox1.SelectedValue.ToString(),
                dateTimePicker1.Value
            );

            frm.Show();
            this.Hide();
        }
    }
}