

using Microsoft.Data.SqlClient; 
using PlasticsApp.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlasticsApp.Services
{
    public class PYMS
    {
        public static string GetLastDehumidifierOfGranulate(string granulate_bc)
        {
            try
            {
                SqlConnection sqlConnection = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                sqlConnection.Open();
                string queryString = $"select Top 1 *, GetDate() as dtNow from tbPlDeHumidifier where granulate_bc = '{granulate_bc}' order by [id] desc";
                SqlCommand command = new SqlCommand(queryString, sqlConnection);
                SqlDataReader reader = command.ExecuteReader();
                string result = "Not found";

                while (reader.Read())
                {
                    result = reader["dehumidifier"].ToString();
                }

                sqlConnection.Close();
                sqlConnection.Dispose();
                return result;
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex);
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro Base de dados", $"{ex}", "Ok");
                return "Not found";
            }
        }

        public static void SetLastGranulateInformationDehumidifier(Dehumidifier dehumidifier)
        {
            try
            {
                SqlConnection sqlConnection = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                sqlConnection.Open();
                string queryString = $"select Top 1 * from tbPlDeHumidifier where DeHumidifier = '{dehumidifier.Name}' order by [id] desc";
                SqlCommand command = new SqlCommand(queryString, sqlConnection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    dehumidifier.GranulateBatch = reader["batch"].ToString();
                    dehumidifier.GranulateBC = reader["granulate_bc"].ToString();
                    dehumidifier.LoadWeight = Convert.ToInt16(reader["weight"]);

                    if (reader["start_date"] != DBNull.Value)
                        dehumidifier.StartDate = Convert.ToDateTime(reader["start_date"]);
                    else
                        dehumidifier.StartDate = DateTime.Now;

                    if (reader["end_date"] != DBNull.Value)
                        dehumidifier.EndDate = Convert.ToDateTime(reader["end_date"]);
                    else
                        dehumidifier.EndDate = DateTime.Now;

                    if (dehumidifier.EndDate > DateTime.Now)
                        dehumidifier.CanBeUsed = true;
                    else
                        dehumidifier.CanBeUsed = false;
                }
                sqlConnection.Close();
                sqlConnection.Dispose();
            }
            catch(SqlException ex)
            {
                Debug.WriteLine(ex);
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro Base de dados", $"{ex}", "Ok");
            } 
        }

        public static void SetLastGranulateInformationDehumidifierSpecificGranulate(Dehumidifier dehumidifier, string lastGranulate)
        {
            try
            {
                SqlConnection sqlConnection = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                sqlConnection.Open();
                string queryString = $"select Top 1 * from tbPlDeHumidifier where DeHumidifier = '{dehumidifier.Name}' and granulate_bc = '{lastGranulate}' order by [id] desc";
                SqlCommand command = new SqlCommand(queryString, sqlConnection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    dehumidifier.GranulateBatch = reader["batch"].ToString();
                    dehumidifier.GranulateBC = reader["granulate_bc"].ToString();
                    dehumidifier.LoadWeight = Convert.ToInt16(reader["weight"]);

                    if (reader["start_date"] != DBNull.Value)
                        dehumidifier.StartDate = Convert.ToDateTime(reader["start_date"]);
                    else
                        dehumidifier.StartDate = DateTime.Now;

                    if (reader["end_date"] != DBNull.Value)
                        dehumidifier.EndDate = Convert.ToDateTime(reader["end_date"]);
                    else
                        dehumidifier.EndDate = DateTime.Now;

                    if (dehumidifier.EndDate < DateTime.Now)
                        dehumidifier.CanBeUsed = true;
                    else
                        dehumidifier.CanBeUsed = false;
                }
                sqlConnection.Close();
                sqlConnection.Dispose();
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex);
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro Base de dados", $"{ex}", "Ok");
            }
        }
        public static void AddDehumidifierLoading(Dehumidifier dehumidifier,bool isWeekend)
        {
            try
            {
                SqlConnection sqlConnection = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                string queryString = "Insert into tbPlDeHumidifier (DeHumidifier, start_date, end_date, granulate_bc, batch, Weight) " +
                $" VALUES ('{dehumidifier.Name}', ";

                // If its stuffing
                if(dehumidifier.StartDate == dehumidifier.EndDate)
                {
                    queryString += " dateAdd(minute ,-1 , getdate()) , dateAdd(minute ,-1 , getdate()) ,";
                } // If its drying
                else
                {
                    if (!isWeekend)
                    {
                        TimeSpan ts = dehumidifier.EndDate.Subtract(dehumidifier.StartDate);
                        queryString += $"getdate(), dateAdd(hour, {ts.Hours}, getdate()), ";
                    }
                    else
                    {
                        TimeSpan ts = dehumidifier.EndDate.Subtract(dehumidifier.StartDate);
                        queryString += $"dateAdd(hour, -{ts.Hours}, getdate()), getdate() ";
                    }
                }          
                // In common information
                queryString += "'" + dehumidifier.GranulateBC + "'," + "'" + dehumidifier.GranulateBatch + "'," + dehumidifier.LoadWeight + ")";


                sqlConnection.Open();

                SqlCommand command = new SqlCommand(queryString, sqlConnection);
                command.ExecuteNonQuery();

                sqlConnection.Close();
            }
            catch(SqlException ex)
            {
                Debug.WriteLine(ex);
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro Base de dados", $"{ex}", "Ok");
            }
        }
        public static bool ChangeProductionOrderBlockStatus(ProductionOrder productionOrder, bool blocked)
        {
            try
            {
                SqlConnection sqlConnection = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                SqlCommand command = new SqlCommand();
                SqlTransaction transaction;

                sqlConnection.Open();
                
                transaction= sqlConnection.BeginTransaction();
                command.Connection = sqlConnection;
                command.CommandType = CommandType.StoredProcedure;

                if (blocked) { command.CommandText = "p_plas_unblock_order_R2"; }
                else { command.CommandText = "p_plas_block_order_R2"; }
                
                command.CommandTimeout = 30;

                command.Parameters.Add("@in_order_id", SqlDbType.Int);
                command.Parameters["@in_order_id"].Value = productionOrder.OrderNumber;

                command.Parameters.Add("@in_pn", SqlDbType.VarChar, 12);
                command.Parameters["@in_pn"].Value = productionOrder.PartNumber;

                command.Parameters.Add("@out_result", SqlDbType.Int);
                command.Parameters["@out_result"].Direction = ParameterDirection.Output;

                Int32 iRecs = command.ExecuteNonQuery();

                if (Convert.ToInt32(command.Parameters["@out_result"].Value) > 0)
                {
                    transaction.Rollback();
                    return false;
                }
                else { transaction.Commit(); }
                     

                sqlConnection.Close();
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex);
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro Base de dados", $"{ex}", "Ok");
            }
            return false;
        }

        public static ProductionOrder GetProductionOrder(int orderNumber)
        {
            ProductionOrder productionOrder = new();
            try
            {
                SqlConnection sqlConnection = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                sqlConnection.Open();
                string queryString = "select tbPlOrders.*, tbPlToolCnt.tool_bc as bc from tbPlOrders left join tbPlToolCnt on tbPlOrders.tool_nr = tbPlToolCnt.tool_nr "
                                   + $"where order_id = {orderNumber}";
                SqlCommand command = new SqlCommand(queryString, sqlConnection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string material = reader["resine"].ToString();
                    string material2 = reader["resine2"].ToString();
                    string materialQuantity= reader["resine_qty"].ToString();
                    string toolNumber = reader["tool_nr"].ToString();
                    string toolBarcode = reader["bc"].ToString(); 
                    int traceId = reader["trace_id"].ToString() == "" ? 0 : Convert.ToInt32(reader["trace_id"].ToString());
                    bool blocked = Convert.ToBoolean(reader["blocked"]);
                    string granulateBarcode = reader["granulate_bc"].ToString();
                    DateTime injectionDate = reader["injection_date"].ToString() == "" ? DateTime.MinValue : Convert.ToDateTime(reader["injection_date"]); 
                    int quantityWIP = reader["qty_wip"].ToString() == "" ? 0 : Convert.ToInt32(reader["qty_wip"].ToString());
                    DateTime dateWIP = reader["wip_date"].ToString() == "" ? DateTime.MinValue : Convert.ToDateTime(reader["wip_date"]);
                    DateTime warehouseDate = reader["plas_wareh_date"].ToString() == "" ? DateTime.MinValue : Convert.ToDateTime(reader["plas_wareh_date"]);
                    string warehouseBarcode = reader["wh_label"].ToString();
                    string mtsBarcode = reader["MTSid"].ToString();

                    productionOrder = new ProductionOrder(material,material2,materialQuantity,toolNumber,toolBarcode,blocked,traceId,granulateBarcode,injectionDate,dateWIP,
                        warehouseDate,warehouseBarcode,mtsBarcode) 
                    {
                        OrderNumber = reader["order_id"].ToString(),
                        OrderId = reader["order_nr"].ToString(),
                        Machine = reader["machine"].ToString(),
                        PartNumber = reader["partnr"].ToString(),
                        Quantity = reader["qty_req"].ToString(),
                        QuantityWIP = reader["qty_wip"] != DBNull.Value ? Convert.ToInt16(reader["qty_wip"]) : 0,
                    };
                }
                sqlConnection.Close();
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex);
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro Base de dados", $"{ex}", "Ok");
            }
            return productionOrder;
        }

        public static ObservableCollection<FailCode> GetFailureCodes()
        {
            ObservableCollection<FailCode> failCodes = new();
            try
            {
                SqlConnection sqlConnection = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                sqlConnection.Open();
                string queryString = "Select * from tbFCodes Where station like 'PL_VINSP' order by fcode_order";
                SqlCommand command = new SqlCommand(queryString, sqlConnection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    failCodes.Add(new FailCode() 
                    { 
                        FullFailCode = reader["fcode"].ToString(),
                        SmallFailCode = reader["fcode"].ToString().Substring(0, 3).ToUpper(),
                        Description = reader["fcode"].ToString().Substring(2, 1)
                    });
                }
                sqlConnection.Close();
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex);
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro Base de dados", $"{ex}", "Ok");
            }
            return failCodes;
        }

        public static bool InsertOrderFailure(ProductionOrder productionOrder, ObservableCollection<OrderFail> orderFails)
        {
            try
            {
                SqlConnection sqlConnection = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                SqlCommand command = new SqlCommand();
                SqlTransaction transaction;

                sqlConnection.Open();

                transaction = sqlConnection.BeginTransaction();

                // Save the information of the non scrap failure codes
                foreach (OrderFail orderFail in orderFails)
                {
                    // If its not scrap
                    if(orderFail.Index != "0")
                    {
                        var response = SaveFtqFails(productionOrder, orderFail.Quantity, orderFail.Code, sqlConnection, transaction);
                        
                        if (response != "")
                        {
                            transaction.Rollback();
                            sqlConnection.Close();
                            sqlConnection.Dispose();

                            var page = App.Current.MainPage;
                            page.DisplayAlert("Erro Base de dados", $"{response}", "Ok");
                            return false;
                        }
                    }
                }

                // Save the log for scrap order fails
                if(orderFails.Where(x => x.Index == "0").Any())
                {
                    var rejectedQuantity = orderFails.Where(x => x.Index == "0").First().Quantity;
                    var response = InternalLogFTQs(productionOrder, rejectedQuantity, rejectedQuantity, sqlConnection, transaction);

                    if (response != "")
                    {
                        transaction.Rollback();
                        sqlConnection.Close();
                        sqlConnection.Dispose();

                        var page = App.Current.MainPage;
                        page.DisplayAlert("Erro Base de dados", $"{response}", "Ok");
                        return false;
                    }
                }

                // Save fail code information to scrap
                foreach (OrderFail orderFail in orderFails)
                {
                    var response = ScrapThisPN(productionOrder.PartNumber, orderFail.Quantity, orderFail.Code);
                    if (response != "")
                    {
                        transaction.Rollback();
                        sqlConnection.Close();
                        sqlConnection.Dispose();

                        var page = App.Current.MainPage;
                        page.DisplayAlert("Erro Base de dados", $"{response}", "Ok");
                        return false;
                    }
                }
                transaction.Commit();
                sqlConnection.Close();
                sqlConnection.Dispose();

                return true;
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex);
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro Base de dados", $"{ex}", "Ok");
                return false;
            }
        }

        public static string ScrapThisPN(string partNumber, int quantity, string ErrorCode)
        {
            try
            {
                SqlConnection sqlConnection = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                SqlCommand command = new SqlCommand();
                SqlTransaction transaction;

                sqlConnection.Open();

                transaction = sqlConnection.BeginTransaction();

                // SAP Insert
                var response = MFBF(partNumber, quantity, sqlConnection, transaction);

                if (response != "")
                {
                    transaction.Rollback();
                    sqlConnection.Close();
                    sqlConnection.Dispose();

                    var page = App.Current.MainPage;
                    page.DisplayAlert("Erro Base de dados", $"{response}", "Ok");
                    return response;
                }

                if (ErrorCode.StartsWith("I00"))
                    response = MB1A(partNumber, ErrorCode, quantity, "0321Val", sqlConnection, transaction);
                else
                    response = MB1A(partNumber, ErrorCode, quantity, "0321", sqlConnection, transaction);

                if (response != "")
                    transaction.Rollback();
                else
                    transaction.Commit();

                sqlConnection.Close();
                sqlConnection.Dispose();

                return "";
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex);
                return ex.Message;
            }
        }

        public static string ScrapThisDirectPN(string partNumber, int quantity)
        {
            try
            {
                SqlConnection sqlConnection = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                SqlCommand command = new SqlCommand();
                SqlTransaction transaction;

                sqlConnection.Open();

                transaction = sqlConnection.BeginTransaction();

                var response = MB1A(partNumber, "", quantity, "0321", sqlConnection, transaction);

                if (response != "")
                    transaction.Rollback();
                else
                    transaction.Commit();

                sqlConnection.Close();
                sqlConnection.Dispose();

                return "";
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex);
                return ex.Message;
            }
        }


        private static string MFBF(string pn, int qty, SqlConnection cn, SqlTransaction trans)
        {
            SqlCommand cmd = new SqlCommand();
            string strResult = "";

            cmd.Connection = cn;
            cmd.Transaction = trans;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "p_SAP_MFBF";
            cmd.CommandTimeout = 30;

            cmd.Parameters.Add("@in_area", SqlDbType.VarChar, 20);
            cmd.Parameters["@in_area"].Value = "PLAS";

            cmd.Parameters.Add("@in_pn", SqlDbType.VarChar, 16);
            cmd.Parameters["@in_pn"].Value = pn;

            cmd.Parameters.Add("@in_qty", SqlDbType.SmallInt);
            cmd.Parameters["@in_qty"].Value = qty;

            try
            {
                Int32 iRecs = cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                strResult = ex.Message;
            }

            return strResult;
        }

        private static string MB1A(string pn, string fail_code, int qty, string area, SqlConnection cn, SqlTransaction trans)
        {
            SqlCommand cmd = new SqlCommand();
            string strResult = "";

            cmd.Connection = cn;
            cmd.Transaction = trans;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "p_SAP_MB1A_r2";
            cmd.CommandTimeout = 30;

            cmd.Parameters.Add("@in_area", SqlDbType.VarChar, 20);
            cmd.Parameters["@in_area"].Value = area;

            cmd.Parameters.Add("@in_station", SqlDbType.VarChar, 30);
            cmd.Parameters["@in_station"].Value = "Moulding";

            cmd.Parameters.Add("@in_pn", SqlDbType.VarChar, 16);
            cmd.Parameters["@in_pn"].Value = pn;

            cmd.Parameters.Add("@in_qty", SqlDbType.SmallInt);
            cmd.Parameters["@in_qty"].Value = qty;

            cmd.Parameters.Add("@in_deposit", SqlDbType.VarChar, 25);
            cmd.Parameters["@in_deposit"].Value = "0321";

            cmd.Parameters.Add("@in_XtraInfo", SqlDbType.VarChar, 20);
            if (fail_code.Length > 20)
                cmd.Parameters["@in_XtraInfo"].Value = fail_code.Substring(0, 20);
            else
                cmd.Parameters["@in_XtraInfo"].Value = fail_code;

            cmd.Parameters.Add("@in_save_now", SqlDbType.Bit);
            cmd.Parameters["@in_save_now"].Value = 1;

            try
            {
                Int32 iRecs = cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                strResult = ex.Message;
            }

            return strResult;
        }

        public static string InternalLogFTQs(ProductionOrder productionOrder, int rejectedQuantity, int producedQuantity, SqlConnection sqlConnection, SqlTransaction transaction)
        {
            try
            {
                SqlCommand command = new SqlCommand();
                command.Connection = sqlConnection;
                command.Transaction = transaction;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "p_plas_inj_vi_FTQ";
                command.CommandTimeout = 30;

                command.Parameters.Add("@in_order_id", SqlDbType.Int);
                command.Parameters["@in_order_id"].Value = productionOrder.OrderNumber;

                command.Parameters.Add("@in_qty_produced", SqlDbType.SmallInt);
                command.Parameters["@in_qty_produced"].Value = producedQuantity;

                command.Parameters.Add("@in_qty_failed", SqlDbType.SmallInt);
                command.Parameters["@in_qty_failed"].Value = rejectedQuantity;

                command.Parameters.Add("@out_result", SqlDbType.Int);
                command.Parameters["@out_result"].Direction = ParameterDirection.Output;

                Int32 iRecs = command.ExecuteNonQuery();
                if (Convert.ToInt32(command.Parameters["@out_result"].Value) > 0)
                {
                    switch (Convert.ToInt32(command.Parameters["@out_result"].Value))
                    {
                        case 1:
                            return "Erros na criação do Produto!";
                        case 2:
                            return "Erros no update da ordem de produção!";
                    }
                }
                return "";
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex);
                return ex.Message;
            }
        }

        public static string SaveFtqFails(ProductionOrder productionOrder, int quantity, string code, SqlConnection sqlConnection, SqlTransaction transaction)
        {
            try
            {
                SqlCommand command = new SqlCommand();
                command.Connection = sqlConnection;
                command.Transaction = transaction;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "p_plas_inj_vi_FTQ_fails_r1";
                command.CommandTimeout = 30;

                command.Parameters.Add("@in_order_id", SqlDbType.Int);
                command.Parameters["@in_order_id"].Value = productionOrder.OrderNumber;

                command.Parameters.Add("@in_pn", SqlDbType.VarChar, 25);
                command.Parameters["@in_pn"].Value = productionOrder.PartNumber;

                string partnumberWithoutZeros = productionOrder.PartNumber.Substring(productionOrder.PartNumber.Length - 8, 8);

                command.Parameters.Add("@in_mc", SqlDbType.VarChar, 12);
                command.Parameters["@in_mc"].Value = partnumberWithoutZeros;

                command.Parameters.Add("@in_fcode", SqlDbType.VarChar, 25);
                if (code.Length > 25)
                    code = code.Substring(0, 25);

                command.Parameters["@in_fcode"].Value = code;

                command.Parameters.Add("@in_qty", SqlDbType.SmallInt);
                command.Parameters["@in_qty"].Value = quantity;

                command.Parameters.Add("@out_result", SqlDbType.Int);
                command.Parameters["@out_result"].Direction = ParameterDirection.Output;

                Int32 iRecs = command.ExecuteNonQuery();
                if (Convert.ToInt32(command.Parameters["@out_result"].Value) > 0)
                {
                    switch (Convert.ToInt32(command.Parameters["@out_result"].Value))
                    {
                        case 1:
                            return "Erros na criação do Produto!";
                        case 2:
                            return "Erros no update da ordem de produção!";
                    }
                }
                return "";
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex);
                return ex.Message;
            }
        }

        public static ObservableCollection<ProductionOrder> GetOrdersForWarehouse(string partNumber, ref int quantityOrders, ref int quantityParts)
        {
            ObservableCollection<ProductionOrder> productionOrders = new();
            quantityOrders = 0;
            quantityParts = 0;

            try
            {
                SqlConnection sqlConnection = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                sqlConnection.Open();

                string queryString = "select order_id, qty_wip, partnr" +
                    " from tbPlOrders " +
                    " Where partnr = '" + partNumber + "'" +
                    " and plas_wareh_date IS NULL " +
                    " and wip_date IS NOT NULL " +
                    " order by order_id";

                SqlCommand command = new SqlCommand(queryString, sqlConnection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    productionOrders.Add(new ProductionOrder() { OrderNumber = reader["order_id"].ToString(), QuantityWIP = Convert.ToInt16(reader["qty_wip"]), PartNumber = reader["partnr"].ToString() });
                    quantityParts += Convert.ToInt16(reader["qty_wip"]);
                    quantityOrders ++;
                }
                sqlConnection.Close();
                sqlConnection.Dispose();
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex);
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro Base de dados", $"{ex}", "Ok");
            }
            return productionOrders;
        }

        public static string MoveToWIP(ProductionOrder productionOrder, int newQuantity)
        {
            SqlConnection cn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            SqlTransaction trans;
            string strResult = "";

            try
            {
                cn.ConnectionString = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20"); 

                cn.Open();
            }
            catch (SqlException)
            {
                return "DBOpenConnection Error";
            }

            trans = cn.BeginTransaction();

            cmd.Connection = cn;
            cmd.Transaction = trans;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "p_plas_WIP_move_in_R2";
            cmd.CommandTimeout = 30;

            cmd.Parameters.Add("@in_order_id", SqlDbType.Int);
            cmd.Parameters["@in_order_id"].Value = productionOrder.OrderNumber;

            cmd.Parameters.Add("@in_qty_wip", SqlDbType.SmallInt);
            cmd.Parameters["@in_qty_wip"].Value = newQuantity;

            cmd.Parameters.Add("@out_result", SqlDbType.Int);
            cmd.Parameters["@out_result"].Direction = ParameterDirection.Output;

            try
            {
                Int32 iRecs = cmd.ExecuteNonQuery();

                if (Convert.ToInt32(cmd.Parameters["@out_result"].Value) > 0)
                {
                    switch (Convert.ToInt32(cmd.Parameters["@out_result"].Value))
                    {
                        case 1:
                            strResult = "Erros no update do procedimento!";
                            break;
                        case 2:
                            strResult = "Ordem não existente!";
                            break;
                        case 3:
                            strResult = "Ordem foi marcada como suspeita!";
                            break;
                        case 4:
                            strResult = "Erro na gravação da rastreabilidade!";
                            break;
                    }

                    trans.Rollback();
                }
                else
                    trans.Commit();

            }
            catch (SqlException ex)
            {
                strResult = ex.Message;

                try
                {
                    trans.Rollback();
                }
                catch (SqlException e)
                {
                    strResult += e.Message;
                }
            }
            finally
            {
                // Always call Close when done reading.
                cn.Close();
                cn.Dispose();
            }
            return strResult;
        }


        public static string ClosePalette(string partNumber)
        {
            SqlConnection cn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            string strResult = "";


            try
            {
                cn.ConnectionString = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");

                cn.Open();
            }
            catch (SqlException)
            {
                return "DBOpenConnection Error";
            }

            cmd.Connection = cn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "Update tbPlOrders " +
                    " SET plas_wareh_date=Getdate() " +
                    " Where partnr = '" + partNumber + "'" +
                    " and plas_wareh_date IS NULL " +
                    " and wip_date IS NOT NULL";

            cmd.CommandTimeout = 30;

            try
            {
                Int32 iRecs = cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                strResult = ex.Message;
            }
            finally
            {
                // Always call Close when done reading.
                cn.Close();
                cn.Dispose();
            }
            return strResult;
        }

        public static string ClosePaletteByOrderNr(ObservableCollection<ProductionOrder> productionOrders)
        {
            SqlConnection cn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            string strResult = "";
            string sql = "";
            int i = 0;

            try
            {
                cn.ConnectionString = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");

                cn.Open();
            }
            catch (SqlException)
            {
                return "DBOpenConnection Error";
            }

            sql = "Update tbPlOrders " +
                    " SET plas_wareh_date=Getdate() " +
                    " Where plas_wareh_date IS NULL " +
                    " and wip_date IS NOT NULL" +
                    " and order_id IN (0,";

            foreach(ProductionOrder productionOrder in productionOrders)
            {
                sql += productionOrder.OrderNumber.ToString() + ",";
            }

            sql = sql.Substring(0, sql.Length - 1) + ")";

            cmd.Connection = cn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            cmd.CommandTimeout = 30;

            try
            {
                Int32 iRecs = cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                strResult = ex.Message;
            }
            finally
            {
                // Always call Close when done reading.
                cn.Close();
                cn.Dispose();
            }

            return strResult;
        }


        public static string GetEventDescriptionByCode(string strEventNr)
        {
            SqlConnection cn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            string strResult = "";

            try
            {
                cn.ConnectionString = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");

                cn.Open();
            }
            catch (SqlException)
            {
                return "Erro na Abertura da Base de dados";
            }

            cmd.Connection = cn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select event from dbo.tbEpsrEventType where [id] = " + strEventNr;
            cmd.CommandTimeout = 30;

            SqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    strResult = reader["event"].ToString();
                }
            }
            finally
            {
                // Always call Close when done reading.
                reader.Close();
                cn.Close();

                cn.Dispose();
            }

            return strResult;
        }


        public static string GetCauses(string event_id, ObservableCollection<string> causesList)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {

                        cn.ConnectionString = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                        cn.Open();


                        cmd.Connection = cn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT reason FROM tbEpsrEventReason WHERE event_id = @event_id";
                        cmd.CommandTimeout = 30;


                        cmd.Parameters.Add("@event_id", SqlDbType.Int).Value = int.Parse(event_id);

                        SqlDataReader reader = cmd.ExecuteReader();


                        while (reader.Read())
                        {
                            string value = reader["reason"] as string;
                            causesList.Add(value);
                        }

                        return "";
                    }
                }
            }
            catch(SqlException ex)
            {
                return ex.Message;
            }
        }


        public static string CheckMouldStatus(string toolNumber)
        {
            SqlConnection cn = new SqlConnection();
            SqlCommand cmd;
            string strSQL;
            string result = "Molde Não Existe!";
             
            try
            {
                cn.ConnectionString = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");

                cn.Open();
            }
            catch (SqlException ex)
            {

                return "Erro na comunicação com o servidor:" + ex.Message + "  => " + ex.InnerException.ToString();
            }


            strSQL = "select * from tbPlToolCnt where tool_nr = '" + toolNumber + "'";
            cmd = new SqlCommand(strSQL, cn);


            SqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    result = "";

                    // Usage Testing
                    if (Convert.ToInt32(reader["act_usage"]) > Convert.ToInt32(reader["red_alarm"]))
                        result = "O molde necessita de Manutenção Preventiva!!!";

                    // Max Stop time testing
                    DateTime T = Convert.ToDateTime(reader["last_time_used"]);
                    T = T.AddDays(Convert.ToInt16(reader["red_alarm_time"]));
                    if (T < DateTime.Now)
                        result = "O molde ultrapassou o tempo de paragem! Necessita de Manutenção Preventiva!!!";


                    // Cumulative Usage Testing
                    if (Convert.ToInt32(reader["cumulative"]) > Convert.ToInt32(reader["end_of_life"]))
                        result = "O Fim de Vida do molde foi atingido!!!";

                    // Usage Testing
                    if (!Convert.ToBoolean(reader["enabled"]))
                        result = "O molde encontra-se bloqueado!!!";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            finally
            {
                // Always call Close when done reading.
                reader.Close();
                cn.Close();

                cn.Dispose();
            }

            return result;
        }

        public static ObservableCollection<ChangeOverProblem> GetChangeOverIssues()
        {
            ObservableCollection <ChangeOverProblem> problems = new();
            try
            {
                using (SqlConnection cn = new SqlConnection("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20"))
                {
                    cn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT id, issue FROM tbPlCOIssue", cn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                problems.Add(new ChangeOverProblem() { Id = Int32.Parse(reader["id"].ToString()) , Issue = reader["issue"].ToString() });
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro Base de dados", $"{e.Message}", "Ok");
            }
            return problems;
        }


        public static TimeSpan GetCOTargetDiff(int stop_id, string station, string runner, string temp, bool robot)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20"))
                {
                    string query = "SELECT dbo.fx_get_CO_target_diff (@in_event_id, @in_station, @in_runner, @in_temp, @in_robot)";

                    using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.Add("@in_event_id", SqlDbType.Int).Value = stop_id;
                        cmd.Parameters.Add("@in_station", SqlDbType.VarChar, 25).Value = station;
                        cmd.Parameters.Add("@in_runner", SqlDbType.VarChar, 10).Value = runner;
                        cmd.Parameters.Add("@in_temp", SqlDbType.VarChar, 10).Value = temp;
                        cmd.Parameters.Add("@in_robot", SqlDbType.Bit).Value = robot;

                        cn.Open();

                        decimal result = (decimal)cmd.ExecuteScalar();

                        cn.Close();

                        return TimeSpan.FromSeconds((double)result);
                    }
                }
            }
            catch (Exception)
            {

                return TimeSpan.FromSeconds(0);
            }
        }



        public static bool HasOpenEvents(string machine)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20"))
                {

                    cn.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = cn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "Select COUNT(stop_id) from v_EPSR_Events_Plas where friendlyName = '" + machine + "' and stop_end IS NULL";
                        cmd.CommandTimeout = 30;

                        int count = (int)cmd.ExecuteScalar();

                        return count > 0;

                    }
                }

            }
            catch (SqlException e)
            {
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro Base de dados", $"{e.Message}", "Ok");
                return false;
            }
        }

        public static ObservableCollection<Event> GetOpenEvents(string machine)
        {
            ObservableCollection<Event> events = new();
            SqlConnection cn = new SqlConnection();
            SqlCommand cmd = new SqlCommand(); 

            try
            {
                cn.ConnectionString = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");

                cn.Open();
            }
            catch (SqlException e)
            {
                var page = App.Current.MainPage;
                page.DisplayAlert("Erro Base de dados", $"{e.Message}", "Ok");
                return events;
            }

            cmd.Connection = cn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "Select * from v_EPSR_Events_Plas where friendlyName = '" + machine + "' and " +
                    "stop_end IS NULL ORDER BY stop_start";
            cmd.CommandTimeout = 30;

            SqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    events.Add(new Event() 
                    {
                        Id = Int32.Parse(reader["class_event_id"].ToString()),
                        Description = reader["event"].ToString(),
                        StopId = Int32.Parse(reader["stop_id"].ToString())
                    });
                }
            }
            finally
            {
                // Always call Close when done reading.
                reader.Close();
                cn.Close();

                cn.Dispose();
            }
            return events;
        }



        public static string UpdateGranulateAndTooling(ProductionOrder pOrder)
        {
            SqlConnection cn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            string strResult = "";


            try
            {
                cn.ConnectionString = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");

                cn.Open();
            }
            catch (SqlException)
            {
                return "DBOpenConnection Error";
            }

            cmd.Connection = cn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "Update tbPLOrders " +
                    "Set granulate_bc='" + pOrder.GranulateBarcode + "', " +
                    "granulate_bc2='" + pOrder.GranulateBarcode2 + "', " +
                    "tool_bc='" + pOrder.ToolBarcode + "' " +
                    "Where order_id = " + pOrder.OrderNumber;
           
            cmd.CommandTimeout = 30;

            try
            {
                Int32 iRecs = cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                strResult = ex.Message;
            }
            finally
            {
                // Always call Close when done reading.
                cn.Close();
                cn.Dispose();
            }

            return strResult;
        }




        public static string StartOfChangeOver(ProductionOrder pOrder)
        {
            SqlConnection cn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            string strResult = "";


            try
            {
                cn.ConnectionString = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");

                cn.Open();
            }
            catch (SqlException)
            {
                return "DBOpenConnection Error";
            }

            cmd.Connection = cn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "Update tbPLOrders " +
                    "Set FirstCOPO=1 " +
                    "Where order_id = " + pOrder.OrderNumber;

            cmd.CommandTimeout = 30;

            try
            {
                Int32 iRecs = cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                strResult = ex.Message;
            }
            finally
            {
                // Always call Close when done reading.
                cn.Close();
                cn.Dispose();
            }

            return strResult;
        }



        public static void EndOfChangeOver(int stop_id, string station, string runner, string temp, bool robot, byte? issueId)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20"))
                {
                    using (SqlCommand cmd = new SqlCommand("p_plas_close_CO", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@in_event_id", SqlDbType.Int).Value = stop_id;
                        cmd.Parameters.Add("@in_station", SqlDbType.VarChar, 25).Value = station;
                        cmd.Parameters.Add("@in_runner", SqlDbType.VarChar, 10).Value = runner;
                        cmd.Parameters.Add("@in_temp", SqlDbType.VarChar, 10).Value = temp;
                        cmd.Parameters.Add("@in_robot", SqlDbType.Bit).Value = robot;
                        cmd.Parameters.Add("@in_issue_id", SqlDbType.TinyInt).Value = ((object)issueId ?? (object)DBNull.Value);

                        cn.Open();

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public static string RecordEventStop(int stop_id)
        {
            SqlConnection cn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            string strResult = "";

            try
            {
                cn.ConnectionString = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                cn.Open();
            }
            catch (SqlException)
            {
                return "DBOpenConnection Error";
            } 

            cmd.Connection = cn;

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "p_Calc_ePSR_CloseEvent";
            cmd.CommandTimeout = 30;

            cmd.Parameters.Add("@stop_id", SqlDbType.Int);
            cmd.Parameters["@stop_id"].Value = stop_id;

            cmd.Parameters.Add("@notes", SqlDbType.VarChar, 100);
            cmd.Parameters["@notes"].Value = "";

            try
            {
                Int32 iRecs = cmd.ExecuteNonQuery(); 
            }
            catch (SqlException ex)
            {
                strResult = ex.Message;
            }
            finally
            {
                // Always call Close when done reading.
                cn.Close();
                cn.Dispose();
            }

            return strResult;
        }


        public static string RecordEventStart(string machine, int event_type, string extra_info, ProductionOrder pOrder)
        {
            SqlConnection cn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            string strResult = "";

            int closed_cavities = 0;

            int line_id;

            //if event_type=14, means cavities closed=> in extra_info comes the number of closed cavities only
            if (event_type == 14)
            {
                try
                {
                    closed_cavities = int.Parse(extra_info);
                }
                catch (Exception)
                {
                    closed_cavities = 0;
                    return "Erro na identificação de cavidades tapadas!";
                }
            }

            try
            {
                cn.ConnectionString = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");

                cn.Open();
            }
            catch (SqlException)
            {
                return "DBOpenConnection Error";
            }

            line_id = GetOELineID(machine);

            //trans = cn.BeginTransaction();

            cmd.Connection = cn;
            //cmd.Transaction = trans;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "p_Calc_ePSR_StartEvent";
            cmd.CommandTimeout = 30;

            cmd.Parameters.Add("@line_id", SqlDbType.Int);
            cmd.Parameters["@line_id"].Value = line_id;

            cmd.Parameters.Add("@prod_part_nr", SqlDbType.VarChar, 15);
            if (pOrder.PartNumber != null)
                cmd.Parameters["@prod_part_nr"].Value = pOrder.PartNumber;
            else
                cmd.Parameters["@prod_part_nr"].Value = null;

            cmd.Parameters.Add("@eventType_id", SqlDbType.Int);
            cmd.Parameters["@eventType_id"].Value = event_type;

            cmd.Parameters.Add("@username", SqlDbType.VarChar, 25);
            cmd.Parameters["@username"].Value = "Symbol_Inj";

            cmd.Parameters.Add("@raw_part_nr", SqlDbType.VarChar, 15);
            if (pOrder.PartNumber != null)
                cmd.Parameters["@raw_part_nr"].Value = pOrder.PartNumber;
            else
                cmd.Parameters["@raw_part_nr"].Value = null;

            cmd.Parameters.Add("@equipment_id", SqlDbType.Int);
            cmd.Parameters["@equipment_id"].Value = -1;

            cmd.Parameters.Add("@nr_closed_cavities", SqlDbType.Int);
            cmd.Parameters["@nr_closed_cavities"].Value = closed_cavities;

            cmd.Parameters.Add("@notes", SqlDbType.VarChar, 100);
            cmd.Parameters["@notes"].Value = extra_info;

            try
            {
                Int32 iRecs = cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                strResult = ex.Message; 
            }
            finally
            {
                // Always call Close when done reading
                cn.Close();
                cn.Dispose();
            }

            return strResult;
        }


        private static int GetOELineID(string machine)
        {
            SqlCommand cmd = new SqlCommand();
            int line_id = -1;
            SqlConnection cn = new SqlConnection();


            try
            {
                cn.ConnectionString = new("user id=passreg;initial catalog=pyms;data source=130.171.191.142;Persist Security Info=True;TrustServerCertificate=True;Encrypt=False;Connection Timeout=20");
                cn.Open();
            }
            catch (SqlException)
            {
                return (-1);
            }

            cmd.Connection = cn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "Select top 1 line_id from tbEpsrLine where friendlyName like '" + machine + "'";
            cmd.CommandTimeout = 30;

            SqlDataReader dr = cmd.ExecuteReader();

            try
            {
                while (dr.Read())
                {
                    line_id = Convert.ToInt16(dr["line_id"]);
                }
                cn.Close();
                return (line_id);
            }
            catch (SqlException)
            {

            }
            cn.Close();
            return (line_id);
        }



    }
}
