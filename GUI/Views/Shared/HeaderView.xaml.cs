using System.Windows;
using System.Windows.Controls;

namespace GUI.Views.Shared
{
    public partial class HeaderView : UserControl
    {
        // === 1: ĐỊNH NGHĨA ROUTED EVENT ===

        // 1.1. Đăng ký một Routed Event mới
        public static readonly RoutedEvent ToggleSidebarClickEvent = EventManager.RegisterRoutedEvent(
            name: "ToggleSidebarClick",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(HeaderView));

        // 1.2. Tạo một trình bao bọc sự kiện .NET chuẩn
        public event RoutedEventHandler ToggleSidebarClick
        {
            add { AddHandler(ToggleSidebarClickEvent, value); }
            remove { RemoveHandler(ToggleSidebarClickEvent, value); }
        }

        // 1.3. Phương thức để phát ra sự kiện
        void RaiseToggleSidebarClickEvent()
        {
            RoutedEventArgs routedEventArgs = new RoutedEventArgs(routedEvent: ToggleSidebarClickEvent);
            RaiseEvent(routedEventArgs);
        }

        public HeaderView()
        {
            InitializeComponent();
        }

        // === 2: KÍCH HOẠT SỰ KIỆN KHI NÚT ĐƯỢC NHẤN ===
        
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Khi nút menu được nhấn, hãy "reo chuông" (phát ra sự kiện)
            RaiseToggleSidebarClickEvent();
        }
        


        // Thêm CLR event chuẩn
        /*
        public event RoutedEventHandler ToggleSidebarClickEventHandler;

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleSidebarClickEventHandler?.Invoke(this, e);
        }
        */

    }
}
