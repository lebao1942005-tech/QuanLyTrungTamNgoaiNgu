using CommunityToolkit.Mvvm.ComponentModel;
using DAL;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GUI.ViewModels
{
    // Class hỗ trợ vẽ biểu đồ (Model cho từng cột)
    public class ChartItem
    {
        public string Label { get; set; }   // Nhãn (VD: 01/2025)
        public decimal Value { get; set; }  // Giá trị thực (VD: 5,000,000)
        public double Height { get; set; }  // Chiều cao cột (tính theo pixel để Binding vào View)
        public string Tooltip { get; set; } // Hiển thị khi rê chuột vào cột
    }

    public partial class DashboardViewModel : ObservableObject
    {
        private readonly DashboardDAL _dal = new DashboardDAL();

        // --- Các chỉ số thống kê tổng quan ---
        [ObservableProperty] private int _totalStudents;
        [ObservableProperty] private decimal _totalRevenue;
        [ObservableProperty] private decimal _totalDebt;
        [ObservableProperty] private double _attendanceRate;

        // --- Dữ liệu biểu đồ (MỚI THÊM) ---
        [ObservableProperty] private ObservableCollection<ChartItem> _revenueChartData;

        public DashboardViewModel()
        {
            // Load dữ liệu bất đồng bộ để không đơ giao diện khi khởi động
            Task.Run(() => LoadDashboardData());
        }

        private void LoadDashboardData()
        {
            try
            {
                // 1. Lấy số liệu tổng quan từ DB
                int students = _dal.GetTotalStudents();
                decimal revenue = _dal.GetTotalRevenue();
                decimal debt = _dal.GetTotalDebt();
                double rate = _dal.GetAttendanceRate();

                // 2. Lấy dữ liệu biểu đồ (Gọi hàm GetRevenueLast6Months từ DAL)
                // Lưu ý: Bạn cần đảm bảo đã thêm hàm này vào DashboardDAL như hướng dẫn trước
                var rawData = _dal.GetRevenueLast6Months();

                // Xử lý tính toán chiều cao cột biểu đồ
                var chartList = new List<ChartItem>();

                // Tìm giá trị lớn nhất để chia tỉ lệ (Max height trên View là 150px)
                decimal maxValue = rawData.Values.Count > 0 ? rawData.Values.Max() : 1;
                if (maxValue == 0) maxValue = 1; // Tránh chia cho 0

                foreach (var kvp in rawData)
                {
                    // Quy đổi doanh thu thành chiều cao pixel (tối đa 150px)
                    double height = (double)(kvp.Value / maxValue) * 150;

                    // Nếu có doanh thu nhưng quá nhỏ, set tối thiểu 5px để người dùng thấy được cột
                    if (height < 5 && kvp.Value > 0) height = 5;

                    chartList.Add(new ChartItem
                    {
                        Label = kvp.Key,
                        Value = kvp.Value,
                        Height = height,
                        Tooltip = $"Tháng {kvp.Key}: {kvp.Value:N0} đ"
                    });
                }

                // 3. Cập nhật lên UI (phải chạy trên UI Thread)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TotalStudents = students;
                    TotalRevenue = revenue;
                    TotalDebt = debt;
                    AttendanceRate = rate;

                    // Cập nhật danh sách biểu đồ
                    RevenueChartData = new ObservableCollection<ChartItem>(chartList);
                });
            }
            catch
            {
                // Xử lý lỗi im lặng hoặc log ra file nếu cần
            }
        }
    }
}