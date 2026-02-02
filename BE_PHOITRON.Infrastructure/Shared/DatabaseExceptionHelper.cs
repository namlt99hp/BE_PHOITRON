using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Linq;

namespace BE_PHOITRON.Infrastructure.Shared
{
    /// <summary>
    /// Helper class để xử lý và parse SQL exceptions thành thông báo rõ ràng cho client
    /// </summary>
    public static class DatabaseExceptionHelper
    {
        /// <summary>
        /// Xử lý exception từ database operations và trả về thông báo lỗi rõ ràng
        /// </summary>
        /// <param name="ex">Exception từ database operation</param>
        /// <returns>Tuple (StatusCode, Message) để trả về cho client</returns>
        public static (int StatusCode, string Message) HandleException(Exception ex)
        {
            // DbUpdateException từ Entity Framework
            if (ex is DbUpdateException dbEx)
            {
                // Kiểm tra inner exception là SqlException
                if (dbEx.InnerException is SqlException sqlEx)
                {
                    return HandleSqlException(sqlEx);
                }
                
                // Nếu không phải SqlException, trả về message mặc định
                return (409, $"Lỗi cập nhật dữ liệu: {dbEx.Message}");
            }

            // SqlException trực tiếp
            if (ex is SqlException sqlExDirect)
            {
                return HandleSqlException(sqlExDirect);
            }

            // InvalidOperationException - business logic errors
            if (ex is InvalidOperationException invalidOpEx)
            {
                return (400, invalidOpEx.Message);
            }

            // Các exception khác
            return (500, $"Đã xảy ra lỗi không xác định: {ex.Message}");
        }

        /// <summary>
        /// Xử lý SqlException cụ thể và trả về thông báo tiếng Việt rõ ràng
        /// </summary>
        private static (int StatusCode, string Message) HandleSqlException(SqlException sqlEx)
        {
            // SQL Server Error Codes
            switch (sqlEx.Number)
            {
                // Foreign Key Constraint Violation
                case 547: // The DELETE/UPDATE statement conflicted with the FOREIGN KEY constraint
                    return HandleForeignKeyViolation(sqlEx);
                
                // Primary Key / Unique Constraint Violation
                case 2627: // Unique constraint violation
                case 2601: // Unique index violation
                    return (409, "Dữ liệu đã tồn tại trong hệ thống. Vui lòng kiểm tra lại mã hoặc thông tin trùng lặp.");
                
                // Cannot insert NULL
                case 515:
                    return (400, "Không thể thêm dữ liệu. Một số trường bắt buộc đang bị thiếu.");
                
                // Deadlock
                case 1205:
                    return (409, "Xung đột dữ liệu. Vui lòng thử lại sau.");
                
                // Timeout
                case -2:
                    return (408, "Thao tác quá thời gian chờ. Vui lòng thử lại.");
                
                default:
                    return (500, $"Lỗi cơ sở dữ liệu (Error {sqlEx.Number}): {sqlEx.Message}");
            }
        }

        /// <summary>
        /// Xử lý Foreign Key Constraint Violation và trả về thông báo chi tiết
        /// </summary>
        private static (int StatusCode, string Message) HandleForeignKeyViolation(SqlException sqlEx)
        {
            string message = sqlEx.Message;
            
            // Parse tên constraint và bảng từ message
            // Format thường: "The DELETE statement conflicted with the REFERENCE constraint \"FK_Name\". 
            //                  The conflict occurred in database \"DB\", table \"dbo.TableName\", column 'ColumnName'."
            
            // Tìm tên constraint
            var constraintMatch = Regex.Match(message, @"constraint\s+""?([^""]+)""?", RegexOptions.IgnoreCase);
            string constraintName = constraintMatch.Success ? constraintMatch.Groups[1].Value : "FK_Unknown";
            
            // Tìm tên bảng bị ảnh hưởng
            var tableMatch = Regex.Match(message, @"table\s+""?([^""]+)""?", RegexOptions.IgnoreCase);
            string tableName = tableMatch.Success ? tableMatch.Groups[1].Value : "bảng không xác định";
            
            // Tìm tên cột
            var columnMatch = Regex.Match(message, @"column\s+""?'?([^""']+)""?'?", RegexOptions.IgnoreCase);
            string columnName = columnMatch.Success ? columnMatch.Groups[1].Value : "cột không xác định";
            
            // Map tên bảng sang tiếng Việt
            string tableNameVi = GetTableNameVietnamese(tableName);
            
            // Xác định loại thao tác (DELETE hay UPDATE)
            bool isDelete = message.Contains("DELETE", StringComparison.OrdinalIgnoreCase);
            string action = isDelete ? "xóa" : "cập nhật";
            
            // Tạo thông báo tiếng Việt
            string friendlyMessage = $"Không thể {action} dữ liệu này. {tableNameVi} đang được sử dụng ở nơi khác trong hệ thống và không thể xóa/cập nhật để đảm bảo tính toàn vẹn dữ liệu.";
            
            return (409, friendlyMessage);
        }

        /// <summary>
        /// Map tên bảng SQL sang tên tiếng Việt
        /// </summary>
        private static string GetTableNameVietnamese(string tableName)
        {
            // Remove schema prefix if exists
            string cleanTableName = tableName.Contains('.') ? tableName.Split('.').Last() : tableName;
            
            return cleanTableName.ToUpper() switch
            {
                "QUANG" => "Quặng",
                "CONG_THUC_PHOI" => "Công thức phối",
                "CTP_CHITIET_QUANG" => "Chi tiết công thức phối",
                "CTP_CHITIET_QUANG_TPHH" => "Thành phần hóa học chi tiết",
                "CTP_RANGBUOC_TPHH" => "Ràng buộc thành phần hóa học",
                "CTP_BANGCHIPHI" => "Bảng chi phí",
                "TP_HOAHOC" => "Thành phần hóa học",
                "PHUONG_AN_PHOI" => "Phương án phối",
                "PA_QUANG_KQ" => "Kết quả quặng phương án",
                "PA_LUACHON_CONGTHUC" => "Lựa chọn công thức",
                "PA_THONGKE_RESULT" => "Kết quả thống kê",
                "PA_SNAPSHOT_TPHH" => "Snapshot thành phần hóa học",
                "PA_KETQUA_TONGHOP" => "Kết quả tổng hợp",
                "QUANG_TP_PHANTICH" => "Thành phần phân tích quặng",
                "QUANG_GIA_LICHSU" => "Lịch sử giá quặng",
                "THONGKE_FUNCTION" => "Hàm thống kê",
                _ => cleanTableName // Fallback to original name
            };
        }

        /// <summary>
        /// Kiểm tra xem exception có phải là Foreign Key violation không
        /// </summary>
        public static bool IsForeignKeyViolation(Exception ex)
        {
            if (ex is DbUpdateException dbEx && dbEx.InnerException is SqlException sqlEx)
            {
                return sqlEx.Number == 547;
            }
            
            if (ex is SqlException sqlExDirect)
            {
                return sqlExDirect.Number == 547;
            }
            
            return false;
        }
    }
}

