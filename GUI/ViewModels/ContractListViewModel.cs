using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAL;
using DTO;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace GUI.ViewModels
{
    public class ContractDisplayDTO
    {
        public int EnrollmentId { get; set; }
        public string StudentName { get; set; }
        public string StudentId { get; set; }
        public string ClassName { get; set; }
        public DateTime EnrollDate { get; set; }
        public string Status { get; set; }
        public int RawStudentId { get; set; }
        public int RawClassId { get; set; } // Cần thêm cái này để bind ComboBox khi sửa
    }

    public partial class ContractListViewModel : ObservableObject
    {
        private readonly EnrollmentBLL _bll = new EnrollmentBLL();
        private readonly ClassBLL _classBLL = new ClassBLL(); // Cần để load danh sách lớp khi sửa

        // --- DATA ---
        private List<ContractDisplayDTO> _allContracts = new List<ContractDisplayDTO>();

        [ObservableProperty] private ObservableCollection<ContractDisplayDTO> _enrollments;
        [ObservableProperty] private ObservableCollection<ClassDTO> _classes; // List lớp cho ComboBox
        [ObservableProperty] private ObservableCollection<string> _statuses = new ObservableCollection<string> { "Active", "Cancelled", "Completed" };

        [ObservableProperty] private string _searchText;
        partial void OnSearchTextChanged(string value) => FilterList();

        // --- EDIT POPUP ---
        [ObservableProperty] private bool _isEditPopupOpen;
        [ObservableProperty] private ContractDisplayDTO _editingContract; // Dùng để hiển thị tên
        [ObservableProperty] private int _editSelectedClassId;
        [ObservableProperty] private DateTime _editEnrollDate;
        [ObservableProperty] private string _editSelectedStatus;

        // --- DELETE POPUP ---
        [ObservableProperty] private bool _isDeletePopupOpen;
        private ContractDisplayDTO _contractToDelete;




        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private int _pageSize = 10; // Số dòng mỗi trang
        [ObservableProperty] private int _totalPages = 0;
        [ObservableProperty] private string _pagingInfo; // Hiển thị "Trang 1 / 5"

        // Danh sách tạm sau khi tìm kiếm (nhưng chưa cắt trang)
        private List<ContractDisplayDTO> _filteredList = new List<ContractDisplayDTO>();

        public ContractListViewModel()
        {
            LoadData();
            LoadClasses(); // Load sẵn danh sách lớp
        }

        private void LoadData()
        {
            try
            {
                var dtEnrollments = _bll.GetAllEnrollments();
                var dtStudents = _bll.GetAllStu();
                var dtClasses = _bll.GetAllClass();

                // Helpers
                var studentDict = new Dictionary<int, (string Name, string Code)>();
                foreach (DataRow row in dtStudents.Rows)
                {
                    int id = Convert.ToInt32(row["StudentID"]);
                    string name = row["Name"].ToString();
                    string code = id.ToString();
                    if (!studentDict.ContainsKey(id)) studentDict.Add(id, (name, code));
                }

                var classDict = new Dictionary<int, string>();
                foreach (DataRow row in dtClasses.Rows)
                {
                    int id = Convert.ToInt32(row["ClassID"]);
                    string name = row["ClassName"].ToString();
                    if (!classDict.ContainsKey(id)) classDict.Add(id, name);
                }

                _allContracts.Clear();
                foreach (DataRow row in dtEnrollments.Rows)
                {
                    int stuId = Convert.ToInt32(row["StudentID"]);
                    int classId = Convert.ToInt32(row["ClassID"]);

                    var studentInfo = studentDict.ContainsKey(stuId) ? studentDict[stuId] : ("Unknown", "N/A");
                    string className = classDict.ContainsKey(classId) ? classDict[classId] : "Unknown Class";

                    _allContracts.Add(new ContractDisplayDTO
                    {
                        EnrollmentId = Convert.ToInt32(row["EnrollmentID"]),
                        RawStudentId = stuId,
                        RawClassId = classId, // Lưu ID lớp
                        StudentName = studentInfo.Item1,
                        StudentId = studentInfo.Item2,
                        ClassName = className,
                        EnrollDate = Convert.ToDateTime(row["EnrollDate"]),
                        Status = row["Status"].ToString()
                    });
                }
                FilterList();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void LoadClasses()
        {
            try
            {
                var list = _classBLL.GetAllClasses();
                Classes = new ObservableCollection<ClassDTO>(list);
            }
            catch { }
        }

        private void FilterList()
        {
            // 1. Lọc dữ liệu theo ô tìm kiếm trước
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                _filteredList = _allContracts; // Lấy hết
            }
            else
            {
                var k = SearchText.ToLower();
                _filteredList = _allContracts.Where(c =>
                    c.EnrollmentId.ToString().Contains(k) ||
                    c.StudentName.ToLower().Contains(k) ||
                    c.ClassName.ToLower().Contains(k)
                ).ToList();
            }

            // 2. Tính toán lại số trang
            if (_filteredList.Count > 0)
            {
                TotalPages = (int)Math.Ceiling((double)_filteredList.Count / PageSize);
            }
            else
            {
                TotalPages = 1;
            }

            // 3. Reset về trang 1 khi tìm kiếm mới
            CurrentPage = 1;

            // 4. Cắt dữ liệu hiển thị
            UpdatePagedCollection();
        }


        private void UpdatePagedCollection()
        {
            // Cập nhật Text hiển thị
            PagingInfo = $"Trang {CurrentPage} / {TotalPages}";

            // Logic Skip & Take
            var pagedData = _filteredList
                            .Skip((CurrentPage - 1) * PageSize)
                            .Take(PageSize)
                            .ToList();

            Enrollments = new ObservableCollection<ContractDisplayDTO>(pagedData);

            // Cập nhật trạng thái nút (để disable khi cần)
            NextPageCommand.NotifyCanExecuteChanged();
            PreviousPageCommand.NotifyCanExecuteChanged();
        }

        // --- COMMANDS CHUYỂN TRANG ---

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                UpdatePagedCollection();
            }
        }
        private bool CanGoNext() => CurrentPage < TotalPages;

        [RelayCommand(CanExecute = nameof(CanGoPrevious))]
        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                UpdatePagedCollection();
            }
        }
        private bool CanGoPrevious() => CurrentPage > 1;



        // ================== LOGIC SỬA ==================
        [RelayCommand]
        private void OpenEditDialog(ContractDisplayDTO contract)
        {
            if (contract == null) return;

            EditingContract = contract;

            // Map dữ liệu cũ vào form
            EditSelectedClassId = contract.RawClassId;
            EditEnrollDate = contract.EnrollDate;
            EditSelectedStatus = contract.Status;

            IsEditPopupOpen = true;
        }

        [RelayCommand]
        private void SaveEdit()
        {
            if (EditingContract == null) return;

            // Tạo DTO cập nhật
            var updateDto = new EnrollmentDTO
            {
                EnrollmentID = EditingContract.EnrollmentId,
                StudentID = EditingContract.RawStudentId, // Giữ nguyên học viên
                ClassID = EditSelectedClassId,
                EnrollDate = EditEnrollDate,
                Status = EditSelectedStatus
            };

            try
            {
                // Gọi BLL Update
                string result = _bll.UpdateEnrollment(updateDto);
                MessageBox.Show(result);

                IsEditPopupOpen = false;
                LoadData(); // Load lại list
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật: " + ex.Message);
            }
        }

        [RelayCommand] private void CancelEdit() => IsEditPopupOpen = false;


        // ================== LOGIC XÓA ==================
        [RelayCommand]
        private void OpenDeleteDialog(ContractDisplayDTO contract)
        {
            if (contract == null) return;
            _contractToDelete = contract;
            IsDeletePopupOpen = true;
        }

        [RelayCommand]
        private void ConfirmDelete()
        {
            if (_contractToDelete == null) return;
            try
            {
                string result = _bll.DeleteEnrollment(_contractToDelete.EnrollmentId);
                MessageBox.Show(result);
                IsDeletePopupOpen = false;
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa: " + ex.Message);
            }
        }

        [RelayCommand] private void CancelDelete() => IsDeletePopupOpen = false;
    }
}