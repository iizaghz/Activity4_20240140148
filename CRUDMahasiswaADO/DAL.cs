using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public class DAL
    {
        public static string GetLoacalIPAddress()
        {
            string localIP = string.Empty;
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting local IP address: " + ex.Message);
            }
            return localIP;
        }

        public static string GetConnectionString()
        {
            string ipAddress = GetLoacalIPAddress();
            
            // 1. Coba menggunakan IP Address + Instance \IZA (dengan Timeout cepat 2 detik)
            string connStrWithInstance = $"Data Source={ipAddress}\\IZA;Initial Catalog=DBAkademikADO;Integrated Security=True;Connection Timeout=2;";
            try
            {
                using (SqlConnection testConn = new SqlConnection(connStrWithInstance))
                {
                    testConn.Open();
                    return connStrWithInstance;
                }
            }
            catch {}

            // 2. Coba menggunakan IP Address saja (jika di-test di default instance PC dosen)
            string connStrDefault = $"Data Source={ipAddress};Initial Catalog=DBAkademikADO;Integrated Security=True;Connection Timeout=2;";
            try
            {
                using (SqlConnection testConn = new SqlConnection(connStrDefault))
                {
                    testConn.Open();
                    return connStrDefault;
                }
            }
            catch {}

            // 3. Fallback ke Server Lokal IZAYAAA\IZA (Pasti berhasil di PC lokal Anda)
            return "Data Source=IZAYAAA\\IZA;Initial Catalog=DBAkademikADO;Integrated Security=True;";
        }

        SqlConnection conn = new SqlConnection(GetConnectionString());

        SqlDataAdapter da;
        DataTable dtMahasiswa;
        DataTable dtProdi;

        public int CountMhs()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlCommand cmd =
                new SqlCommand("sp_CountMahasiswa", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            SqlParameter outputParam =
                new SqlParameter("@Total", SqlDbType.Int);

            outputParam.Direction =
                ParameterDirection.Output;

            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            return Convert.ToInt32(outputParam.Value);
        }

        public DataTable GetMhs()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlCommand cmd =
                new SqlCommand("sp_GetMahasiswa", conn);

            cmd.CommandType =
                CommandType.StoredProcedure;

            da = new SqlDataAdapter(cmd);

            dtMahasiswa = new DataTable();

            da.Fill(dtMahasiswa);

            return dtMahasiswa;
        }


        public void InsertMhs(
            string nim,
            string nama,
            string alamat,
            string jenisKelamin,
            DateTime tanggalLahir,
            string kodeProdi,
            byte[] foto)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                SqlCommand command =
                    new SqlCommand("sp_InsertMahasiswa", conn, trans);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@pNIM", nim);
                command.Parameters.AddWithValue("@pNama", nama);
                command.Parameters.AddWithValue("@pAlamat", alamat);
                command.Parameters.AddWithValue("@pTanggalLahir", tanggalLahir);
                command.Parameters.AddWithValue("@pJenisKelamin", jenisKelamin);
                command.Parameters.AddWithValue("@pKodeProdi", kodeProdi);
                command.Parameters.AddWithValue("@pFoto", foto);

                command.ExecuteNonQuery();

                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
                throw;
            }
            finally
            {
                conn.Close();
            }
        }

        public void UpdateMhs(
            string nim,
            string nama,
            string alamat,
            string jenisKelamin,
            DateTime tanggalLahir,
            string kodeProdi,
            byte[] foto)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlCommand command =
                new SqlCommand("sp_UpdateMahasiswa", conn);

            command.Parameters.AddWithValue("@pNIM", nim);
            command.Parameters.AddWithValue("@pNama", nama);
            command.Parameters.AddWithValue("@pAlamat", alamat);
            command.Parameters.AddWithValue("@pJenisKelamin", jenisKelamin);
            command.Parameters.AddWithValue("@pTanggalLahir", tanggalLahir);
            command.Parameters.AddWithValue("@pKodeProdi", kodeProdi);
            command.Parameters.AddWithValue("@pFoto", foto);

            command.CommandType = CommandType.StoredProcedure;

            command.ExecuteNonQuery();

            conn.Close();
        }

        public void DeleteMhs(string nim)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlCommand cmd =
                new SqlCommand("sp_DeleteMahasiswa", conn);

            cmd.Parameters.AddWithValue("@pNIM", nim);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.ExecuteNonQuery();

            conn.Close();
        }

        public void resetData()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            string deleteQuery = "DELETE FROM mahasiswa;";

            SqlCommand cmdDelete =
                new SqlCommand(deleteQuery, conn);

            cmdDelete.ExecuteNonQuery();

            string insertQuery = @"
        INSERT INTO mahasiswa
        SELECT * FROM mahasiswa_backup;";

            SqlCommand cmdInsert =
                new SqlCommand(insertQuery, conn);

            cmdInsert.ExecuteNonQuery();

            conn.Close();
        }

        public void testInject(string nim)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            string query =
                "UPDATE mahasiswa SET nama = 'HACKED' WHERE NIM = '" + nim + "'";

            SqlCommand cmd =
                new SqlCommand(query, conn);

            cmd.ExecuteNonQuery();

            conn.Close();
        }

        public DataTable GetMhsByNIM(string nim)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlCommand cmd =
                new SqlCommand("sp_GetMahasiswaByNIM", conn);

            cmd.CommandType =
                CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@pNIM", nim);

            da = new SqlDataAdapter(cmd);

            dtMahasiswa = new DataTable();

            da.Fill(dtMahasiswa);

            return dtMahasiswa;
        }

        public void InsertLog(string message)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlCommand cmd =
                new SqlCommand("sp_LogMessage", conn);

            cmd.Parameters.AddWithValue("@psn", message);

            cmd.CommandType =
                CommandType.StoredProcedure;

            cmd.ExecuteNonQuery();

            conn.Close();
        }

        public DataTable getProdi()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlCommand cmd =
                new SqlCommand("select namaprodi from programstudi", conn);

            cmd.CommandType = CommandType.Text;

            dtProdi = new DataTable();

            da = new SqlDataAdapter(cmd);

            da.Fill(dtProdi);

            return dtProdi;
        }
        public DataTable getDataRekap(string prodi, DateTime tanggalMasuk)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlCommand cmd =
                new SqlCommand("sp_Report", conn);

            cmd.CommandType =
                CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@inProdi", prodi);

            cmd.Parameters.AddWithValue(
                "@inTglMsuk",
                tanggalMasuk.Year.ToString());

            da = new SqlDataAdapter(cmd);

            dtMahasiswa = new DataTable();

            da.Fill(dtMahasiswa);

            return dtMahasiswa;
        }

        public DataTable getAllDataChart()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlCommand cmd =
                new SqlCommand("sp_DashBoard", conn);

            cmd.CommandType =
                CommandType.StoredProcedure;

            da = new SqlDataAdapter(cmd);

            dtMahasiswa = new DataTable();

            da.Fill(dtMahasiswa);

            return dtMahasiswa;
        }

        public DataTable getDataChartByTahun(DateTime thMasuk)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlCommand cmd =
                new SqlCommand("sp_DashBoardByTahun", conn);

            cmd.CommandType =
                CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@inTglMsuk",
                thMasuk.Year);

            da = new SqlDataAdapter(cmd);

            dtMahasiswa = new DataTable();

            da.Fill(dtMahasiswa);

            return dtMahasiswa;
        }
    }
}

