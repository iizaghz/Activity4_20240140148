using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CRUDMahasiswaADO
{
    public class DAL
    {
        SqlConnection conn = new SqlConnection(GetConnectionString());
        SqlDataAdapter da;
        DataTable dtMahasiswa;
        DataTable dtProdi;

        public static string GetLoacalIPAddress()
        {
            // Mengembalikan hostname server "IZAYAAA" agar client di jaringan lokal terhubung ke database server.
            return "IZAYAAA";
        }

        public static string GetConnectionString()
        {
            string connectionString = $"Data Source={GetLoacalIPAddress()}\\IZA;Initial Catalog=DBAkademikADO;User ID=mhs;Password=mhs123;";
            return connectionString;
        }

        public Dictionary<string, string> GetProdiMap()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (SqlConnection tempConn = new SqlConnection(GetConnectionString()))
            {
                tempConn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT KodeProdi, NamaProdi FROM ProgramStudi", tempConn))
                {
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            string code = rdr["KodeProdi"]?.ToString()?.Trim() ?? string.Empty;
                            string name = rdr["NamaProdi"]?.ToString()?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(code))
                            {
                                map[code] = code;
                                if (!string.IsNullOrEmpty(name))
                                {
                                    map[name] = code;
                                }
                            }
                        }
                    }
                }
            }
            return map;
        }

        public int CountMhs()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            try
            {
                SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int);
                outputParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outputParam);

                cmd.ExecuteNonQuery();

                return Convert.ToInt32(outputParam.Value);
            }
            finally
            {
                conn.Close();
            }
        }

        public DataTable GetMhs()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            try
            {
                SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                return dtMahasiswa;
            }
            finally
            {
                conn.Close();
            }
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
                SqlCommand command = new SqlCommand("sp_InsertMahasiswa", conn, trans);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@pNIM", nim);
                command.Parameters.AddWithValue("@pNama", nama);
                command.Parameters.AddWithValue("@pAlamat", alamat);
                command.Parameters.AddWithValue("@pTanggalLahir", tanggalLahir);
                command.Parameters.AddWithValue("@pJenisKelamin", jenisKelamin);
                command.Parameters.AddWithValue("@pKodeProdi", kodeProdi);
                
                // Penanganan nilai null pada foto sesuai standar database agar tidak terjadi error konversi tipe data
                if (foto == null)
                {
                    command.Parameters.Add("@pFoto", SqlDbType.VarBinary, -1).Value = DBNull.Value;
                }
                else
                {
                    command.Parameters.AddWithValue("@pFoto", foto);
                }

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

            try
            {
                SqlCommand command = new SqlCommand("sp_UpdateMahasiswa", conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@pNIM", nim);
                command.Parameters.AddWithValue("@pNama", nama);
                command.Parameters.AddWithValue("@pAlamat", alamat);
                command.Parameters.AddWithValue("@pJenisKelamin", jenisKelamin);
                command.Parameters.AddWithValue("@pTanggalLahir", tanggalLahir);
                command.Parameters.AddWithValue("@pKodeProdi", kodeProdi);

                if (foto == null)
                {
                    command.Parameters.Add("@pFoto", SqlDbType.VarBinary, -1).Value = DBNull.Value;
                }
                else
                {
                    command.Parameters.AddWithValue("@pFoto", foto);
                }

                command.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }

        public void DeleteMhs(string nim)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            try
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@pNIM", nim);

                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }

        public void resetData()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                SqlCommand cmdDelete = new SqlCommand("delete from mahasiswa;", conn, trans);
                cmdDelete.ExecuteNonQuery();

                SqlCommand cmdInsert = new SqlCommand("insert into mahasiswa (NIM, Nama, JenisKelamin, TanggalLahir, Alamat, KodeProdi, TanggalDaftar, Foto) select NIM, Nama, JenisKelamin, TanggalLahir, Alamat, KodeProdi, TanggalDaftar, NULL from mahasiswa_backup;", conn, trans);
                cmdInsert.ExecuteNonQuery();

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

        public void testInject(string nim)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            try
            {
                string query = "UPDATE mahasiswa SET nama = 'HACKED' WHERE NIM = '" + nim + "'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }

        public DataTable GetMhsByNIM(string nim)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            try
            {
                SqlCommand cmd = new SqlCommand("sp_GetMahasiswaByNIM", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@pNIM", nim);

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                return dtMahasiswa;
            }
            finally
            {
                conn.Close();
            }
        }

        public void InsertLog(string message)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            try
            {
                SqlCommand cmd = new SqlCommand("sp_LogMessage", conn);
                cmd.Parameters.AddWithValue("@psn", message);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }

        public DataTable getProdi()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            try
            {
                SqlCommand cmd = new SqlCommand("select namaprodi from programstudi", conn);
                cmd.CommandType = CommandType.Text;

                dtProdi = new DataTable();
                da = new SqlDataAdapter(cmd);
                da.Fill(dtProdi);

                return dtProdi;
            }
            finally
            {
                conn.Close();
            }
        }

        public DataTable getDataRekap(string prodi, DateTime tanggalMasuk)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            try
            {
                SqlCommand cmd = new SqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@inProdi", prodi);
                cmd.Parameters.AddWithValue("@inTglMsuk", tanggalMasuk.Year.ToString());

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                return dtMahasiswa;
            }
            finally
            {
                conn.Close();
            }
        }

        public DataTable getAllDataChart()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            try
            {
                SqlCommand cmd = new SqlCommand("sp_DashBoard", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                return dtMahasiswa;
            }
            finally
            {
                conn.Close();
            }
        }

        public DataTable getDataChartByTahun(DateTime thMasuk)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            try
            {
                SqlCommand cmd = new SqlCommand("sp_DashBoardByTahun", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@inTglMsuk", thMasuk.Year);

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                return dtMahasiswa;
            }
            finally
            {
                conn.Close();
            }
        }

        public string GetKodeProdiByNama(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            try
            {
                SqlCommand cmd = new SqlCommand("SELECT TOP 1 KodeProdi FROM ProgramStudi WHERE NamaProdi = @input", conn);
                cmd.Parameters.AddWithValue("@input", input);
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return result.ToString().Trim();
                }

                SqlCommand cmd2 = new SqlCommand("SELECT TOP 1 KodeProdi FROM ProgramStudi WHERE KodeProdi = @input", conn);
                cmd2.Parameters.AddWithValue("@input", input);
                object result2 = cmd2.ExecuteScalar();
                if (result2 != null)
                {
                    return result2.ToString().Trim();
                }
            }
            finally
            {
                conn.Close();
            }

            return input;
        }
    }
}
