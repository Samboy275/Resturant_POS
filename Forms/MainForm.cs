using POS.Controllers;
using POS.Models;
using POS.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using POS.Enums;

namespace POS.Forms
{
    public partial class MainForm : Form
    {
        // --- Dependencies injected via constructor ---
        private MenuController _menuController;
        private OrderController _orderController;
        private PrintService _printService;

        // --- Application State ---
        private User _currentUser; // The user logged in
        private Order _currentOrder; // The order currently being built
        private List<Category> _categories; // Cached list of categories
        private List<MenuItem> _currentMenuItems; // Menu items for the currently selected category

        // --- UI Controls (declared to be accessible throughout the class) ---
        private Panel categoriesPanel;
        private FlowLayoutPanel categoriesFlowPanel; // For flexible category button layout
        private Panel menuItemsPanel;
        private FlowLayoutPanel menuItemsFlowPanel; // For flexible menu item button layout
        private Panel orderPanel;
        private ListView orderListView;
        private Label totalLabel;
        private Button takeAwayButton;
        private Button deliveryButton;
        private Button payButton;
        private Button adminButton;
        private Button reportsButton;
        private ComboBox orderTypeCombo;
        private Panel deliveryInfoPanel;
        private TextBox customerNameBox;
        private TextBox customerPhoneBox;
        private TextBox deliveryAddressBox;
        private Label statusLabel; // To display order status/messages

        // --- Constructor (designed for Dependency Injection) ---
        // The DI container (configured in Program.cs) will provide instances of MenuController,
        // OrderController, and PrintService when MainForm is created.
        public MainForm(MenuController menuController, OrderController orderController, PrintService printService)
        {
            _menuController = menuController;
            _orderController = orderController;
            _printService = printService;

            // This is for manual UI creation. If using the WinForms designer,
            // ensure your designer-generated InitializeComponent() is called here.
            InitializeManualComponents();

            // _currentUser, _currentOrder, and category/menu item loading
            // are handled by SetCurrentUser after the form is fully constructed and the user is known.
        }

        /// <summary>
        /// Sets the currently logged-in user and initializes parts of the UI dependent on user data.
        /// This method should be called immediately after the MainForm instance is created via DI.
        /// </summary>
        /// <param name="user">The authenticated user.</param>
        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            this.Text = $"Restaurant POS - {_currentUser.FullName}"; // Update form title
            InitializeNewOrder(); // Start a fresh order for the logged-in user
            LoadCategoriesAsync(); // Load categories for the menu
            // Show/hide admin button based on user role
            if (adminButton != null)
            {
                adminButton.Visible = (_currentUser.Role == Role.Admin);
            }
        }

        // --- Core UI Initialization (Manual - if not using designer) ---
        // If you're using the WinForms designer, your MainForm.Designer.cs will contain
        // an InitializeComponent() method. In that case, you'd call that method
        // in the constructor, and move your custom layout code into the designer file
        // or call these CreateXPanel methods from the designer.
        private void InitializeManualComponents()
        {
            this.Size = new Size(1200, 800);
            this.Text = "Restaurant POS"; // Initial text, updated by SetCurrentUser
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            CreateMainLayout();
            CreateCategoriesPanel();
            CreateMenuItemsPanel();
            CreateOrderPanel();
            CreateDeliveryInfoPanel();
        }

        private void CreateMainLayout()
        {
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                // These styles ensure the columns take percentages of the form's width
                // 25% for categories, 45% for menu items, 30% for the order
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 25F),
                    new ColumnStyle(SizeType.Percent, 45F),
                    new ColumnStyle(SizeType.Percent, 30F)
                }
            };
            this.Controls.Add(mainContainer);

            // Left Column: Categories Panel
            categoriesPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray,
                Padding = new Padding(10)
            };
            mainContainer.Controls.Add(categoriesPanel, 0, 0);

            // Middle Column: Menu Items Panel
            menuItemsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10),
                AutoScroll = true // Enable scrolling if many items
            };
            mainContainer.Controls.Add(menuItemsPanel, 1, 0);

            // Right Column: Order Panel
            orderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(10)
            };
            mainContainer.Controls.Add(orderPanel, 2, 0);
        }

        private void CreateCategoriesPanel()
        {
            var titleLabel = new Label
            {
                Text = "CATEGORIES",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.DarkSlateGray,
                ForeColor = Color.White
            };
            categoriesPanel.Controls.Add(titleLabel);

            // FlowLayoutPanel to hold category buttons dynamically
            categoriesFlowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown, // Arrange buttons vertically
                AutoScroll = true, // Enable scrolling if many categories
                WrapContents = false, // Prevents wrapping to next column if TopDown
                Padding = new Padding(5),
                Location = new Point(0, titleLabel.Height) // Position below title
            };
            categoriesPanel.Controls.Add(categoriesFlowPanel);
            categoriesFlowPanel.BringToFront(); // Ensure it's visually on top
        }

        private void CreateMenuItemsPanel()
        {
            var titleLabel = new Label
            {
                Text = "MENU ITEMS",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.DarkSlateGray,
                ForeColor = Color.White
            };
            menuItemsPanel.Controls.Add(titleLabel);

            // FlowLayoutPanel to hold menu item buttons dynamically
            menuItemsFlowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight, // Arrange buttons horizontally first
                AutoScroll = true, // Enable scrolling if many items
                WrapContents = true, // Allows items to wrap to the next row
                Padding = new Padding(5),
                Location = new Point(0, titleLabel.Height)
            };
            menuItemsPanel.Controls.Add(menuItemsFlowPanel);
            menuItemsFlowPanel.BringToFront();
        }

        private void CreateOrderPanel()
        {
            // Panels to divide the order panel into top, middle, and bottom sections
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(5) };
            var panelMiddle = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 200, Padding = new Padding(5) };

            // Add controls to orderPanel in reverse order for proper docking
            orderPanel.Controls.Add(panelBottom);
            orderPanel.Controls.Add(panelMiddle);
            orderPanel.Controls.Add(panelTop);


            // --- Top Section: Order Type, New Order Button, Status Label ---
            orderTypeCombo = new ComboBox
            {
                DataSource = Enum.GetValues(typeof(OrderType)), // Populate with OrderType enum values
                DropDownStyle = ComboBoxStyle.DropDownList, // Prevent manual text entry
                Location = new Point(5, 10),
                Width = 120
            };
            orderTypeCombo.SelectedIndexChanged += OrderTypeCombo_SelectedIndexChanged; // Event handler for type change
            panelTop.Controls.Add(orderTypeCombo);

            var newOrderButton = new Button
            {
                Text = "New Order",
                Location = new Point(orderTypeCombo.Right + 10, 10),
                Width = 100,
                Height = 30,
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            newOrderButton.Click += (s, e) => InitializeNewOrder(); // Reset order on click
            panelTop.Controls.Add(newOrderButton);

            statusLabel = new Label
            {
                Text = "Status: New Order",
                Font = new Font("Arial", 10, FontStyle.Italic),
                Location = new Point(5, 50),
                AutoSize = true // Adjusts size automatically based on text
            };
            panelTop.Controls.Add(statusLabel);


            // --- Middle Section: Order ListView ---
            orderListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details, // Show columns and rows
                FullRowSelect = true, // Selects the entire row
                GridLines = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable // Prevent accidental sorting
            };
            // Define columns for the order list view
            orderListView.Columns.Add("Item", 150);
            orderListView.Columns.Add("Qty", 50, HorizontalAlignment.Center);
            orderListView.Columns.Add("Price", 80, HorizontalAlignment.Right);
            orderListView.Columns.Add("Line Total", 100, HorizontalAlignment.Right);
            orderListView.DoubleClick += OrderListView_DoubleClick; // Event to remove/reduce items
            panelMiddle.Controls.Add(orderListView);

            // --- Bottom Section: Total Label, Action Buttons ---
            totalLabel = new Label
            {
                Text = "TOTAL: $0.00",
                Font = new Font("Arial", 20, FontStyle.Bold),
                Location = new Point(5, 5),
                AutoSize = true,
                ForeColor = Color.DarkGreen
            };
            panelBottom.Controls.Add(totalLabel);

            // Payment Button
            payButton = new Button
            {
                Text = "Pay",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(120, 60),
                Location = new Point(5, totalLabel.Bottom + 10),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            payButton.Click += HandlePayButtonClick;
            panelBottom.Controls.Add(payButton);

            // Order Type Buttons (as alternatives to ComboBox, or just for quick changes)
            takeAwayButton = new Button
            {
                Text = "Take Away",
                Font = new Font("Arial", 10),
                Size = new Size(100, 40),
                Location = new Point(payButton.Right + 10, payButton.Top + 10),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            takeAwayButton.Click += HandleTakeAwayButtonClick;
            panelBottom.Controls.Add(takeAwayButton);

            deliveryButton = new Button
            {
                Text = "Delivery",
                Font = new Font("Arial", 10),
                Size = new Size(100, 40),
                Location = new Point(takeAwayButton.Right + 10, payButton.Top + 10),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            deliveryButton.Click += HandleDeliveryButtonClick;
            panelBottom.Controls.Add(deliveryButton);

            // Admin Button (visibility based on user role)
            adminButton = new Button
            {
                Text = "Admin",
                Font = new Font("Arial", 10),
                Size = new Size(90, 40),
                Location = new Point(panelBottom.Width - 100, 10), // Align to right
                Anchor = AnchorStyles.Top | AnchorStyles.Right, // Keep it aligned to right on resize
                BackColor = Color.OrangeRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            adminButton.Click += AdminButton_Click;
            panelBottom.Controls.Add(adminButton);

            // Reports Button
            reportsButton = new Button
            {
                Text = "Reports",
                Font = new Font("Arial", 10),
                Size = new Size(90, 40),
                Location = new Point(panelBottom.Width - 100, adminButton.Bottom + 5), // Below admin button
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            reportsButton.Click += ReportsButton_Click;
            panelBottom.Controls.Add(reportsButton);

            // Adjust button positions on panel resize
            panelBottom.Resize += (s, e) =>
            {
                adminButton.Location = new Point(panelBottom.Width - 100, 10);
                reportsButton.Location = new Point(panelBottom.Width - 100, adminButton.Bottom + 5);
            };
        }

        private void CreateDeliveryInfoPanel()
        {
            deliveryInfoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightYellow,
                Padding = new Padding(10),
                Visible = false // Hidden by default, shown for Delivery orders
            };

            // Using TableLayoutPanel for organized input fields
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 2,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 30F),
                    new ColumnStyle(SizeType.Percent, 70F)
                },
                RowStyles =
                {
                    new RowStyle(SizeType.AutoSize),
                    new RowStyle(SizeType.AutoSize),
                    new RowStyle(SizeType.AutoSize),
                    new RowStyle(SizeType.AutoSize)
                }
            };

            tableLayout.Controls.Add(new Label { Text = "Customer Name:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            customerNameBox = new TextBox { Dock = DockStyle.Fill };
            tableLayout.Controls.Add(customerNameBox, 1, 0);

            tableLayout.Controls.Add(new Label { Text = "Phone:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            customerPhoneBox = new TextBox { Dock = DockStyle.Fill };
            tableLayout.Controls.Add(customerPhoneBox, 1, 1);
            customerPhoneBox.Leave += CustomerPhoneBox_Leave; // Event to search for customer on leaving field

            tableLayout.Controls.Add(new Label { Text = "Address:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            deliveryAddressBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 60 };
            tableLayout.Controls.Add(deliveryAddressBox, 1, 2);

            deliveryInfoPanel.Controls.Add(tableLayout);

            // Add this panel to the orderPanel. When visible, it will overlay the order list.
            orderPanel.Controls.Add(deliveryInfoPanel);
            deliveryInfoPanel.BringToFront(); // Ensure it appears on top when Visible = true
        }

        // --- Data Loading and Display Logic ---

        /// <summary>
        /// Initializes a new, empty order for the current user.
        /// </summary>
        private void InitializeNewOrder()
        {
            _currentOrder = new Order
            {
                CreatedAt = DateTime.Now,
                OrderType = OrderType.Takeaway, // Default to Takeaway
                OrderStatus = OrderStatus.Pending,
                UserId = _currentUser.Id,
                User = _currentUser,
                OrderItems = new List<OrderItem>()
            };
            _currentOrder.CalculateTotal(); // Ensure total is 0.00 initially

            orderListView.Items.Clear(); // Clear the UI list view
            UpdateOrderSummary(); // Refresh total display
            orderTypeCombo.SelectedItem = OrderType.Takeaway; // Set combo box selection
            deliveryInfoPanel.Visible = false; // Hide delivery info by default
            statusLabel.Text = "Status: New Order (Take Away)";
            customerNameBox.Clear();
            customerPhoneBox.Clear();
            deliveryAddressBox.Clear();
            _currentOrder.CustomerId = null; // Clear any previous customer data
            _currentOrder.Customer = null;
        }

        /// <summary>
        /// Asynchronously loads all categories from the database and displays them.
        /// </summary>
        private async void LoadCategoriesAsync()
        {
            try
            {
                _categories = (await _menuController.GetCategoriesAsync()).ToList();
                DisplayCategories();
                // Select and display menu items for the first category by default
                if (_categories.Any())
                {
                    HandleCategoryClick(_categories.First());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Populates the categoriesFlowPanel with buttons for each category.
        /// </summary>
        private void DisplayCategories()
        {
            categoriesFlowPanel.Controls.Clear();
            foreach (var category in _categories)
            {
                var categoryButton = new Button
                {
                    Text = category.Name,
                    Tag = category.Id, // Store Category ID for easy lookup
                    Size = new Size(categoriesFlowPanel.Width - 10, 60), // Almost full width of panel
                    Margin = new Padding(5),
                    BackColor = ColorTranslator.FromHtml(category.Color), // Use category's color
                    ForeColor = Color.White,
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance.BorderSize = 0
                };
                categoryButton.Click += (sender, e) => HandleCategoryClick(category);
                categoriesFlowPanel.Controls.Add(categoryButton);
            }
        }

        /// <summary>
        /// Asynchronously loads menu items for a given category and displays them.
        /// </summary>
        /// <param name="categoryId">The ID of the category to display menu items for.</param>
        private async void DisplayMenuItems(int categoryId)
        {
            menuItemsFlowPanel.Controls.Clear();
            try
            {
                _currentMenuItems = (await _menuController.GetMenuItemsByCategoryIdAsync(categoryId)).ToList();

                foreach (var menuItem in _currentMenuItems)
                {
                    var menuItemButton = new Button
                    {
                        Text = $"{menuItem.Name}\n${menuItem.Price:N2}", // Display name and price
                        Tag = menuItem.Id, // Store MenuItem ID
                        Size = new Size(150, 100), // Standard button size
                        Margin = new Padding(5),
                        BackColor = ColorTranslator.FromHtml(menuItem.Color),
                        ForeColor = Color.White,
                        Font = new Font("Arial", 10, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        FlatAppearance.BorderSize = 0
                    };
                    menuItemButton.Click += (sender, e) => HandleMenuItemClick(menuItem);
                    menuItemsFlowPanel.Controls.Add(menuItemButton);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading menu items: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Adds a menu item to the current order or increments its quantity if already present.
        /// </summary>
        /// <param name="item">The MenuItem to add.</param>
        private void AddOrderItem(MenuItem item)
        {
            var existingOrderItem = _currentOrder.OrderItems.FirstOrDefault(oi => oi.MenuItemId == item.Id);

            if (existingOrderItem != null)
            {
                existingOrderItem.Quantity++;
            }
            else
            {
                _currentOrder.OrderItems.Add(new OrderItem
                {
                    MenuItemId = item.Id,
                    ItemName = item.Name,
                    Price = item.Price, // Store price at time of order for historical accuracy
                    Quantity = 1
                });
            }
            UpdateOrderSummary(); // Refresh UI
        }

        /// <summary>
        /// Updates the order ListView and the total label based on the current order.
        /// </summary>
        private void UpdateOrderSummary()
        {
            orderListView.Items.Clear();
            foreach (var item in _currentOrder.OrderItems)
            {
                var lvItem = new ListViewItem(item.ItemName);
                lvItem.SubItems.Add(item.Quantity.ToString());
                lvItem.SubItems.Add(item.Price.ToString("N2"));
                lvItem.SubItems.Add(item.LineTotal.ToString("N2"));
                lvItem.Tag = item; // Store the actual OrderItem object for easy retrieval on double-click
                orderListView.Items.Add(lvItem);
            }

            _currentOrder.CalculateTotal(); // Recalculate total from current items
            totalLabel.Text = $"TOTAL: ${_currentOrder.Total:N2}";
        }

        /// <summary>
        /// Removes an item from the order or reduces its quantity.
        /// (Currently handled via OrderListView_DoubleClick)
        /// </summary>
        /// <param name="itemToRemove">The OrderItem to remove/reduce.</param>
        private void RemoveOrderItem(OrderItem itemToRemove)
        {
            if (itemToRemove.Quantity > 1)
            {
                itemToRemove.Quantity--;
            }
            else
            {
                _currentOrder.OrderItems.Remove(itemToRemove);
            }
            UpdateOrderSummary();
        }

        // --- Event Handlers ---

        /// <summary>
        /// Handles a click on a category button, displaying its menu items.
        /// </summary>
        private void HandleCategoryClick(Category category)
        {
            DisplayMenuItems(category.Id);

            // Optional: Visually highlight the selected category button
            foreach (Control control in categoriesFlowPanel.Controls)
            {
                if (control is Button btn && btn.Tag is int tagId)
                {
                    // Reset styling for all buttons
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = ColorTranslator.FromHtml(_categories.FirstOrDefault(c => c.Id == tagId)?.Color ?? "#808080"); // Revert to original color
                }
            }
            // Apply highlight to the clicked button
            var clickedButton = categoriesFlowPanel.Controls.OfType<Button>()
                                .FirstOrDefault(b => b.Tag as int? == category.Id);
            if (clickedButton != null)
            {
                clickedButton.FlatAppearance.BorderSize = 3; // Thicker border
                clickedButton.FlatAppearance.BorderColor = Color.White; // White border for highlight
                clickedButton.BackColor = Color.FromArgb(clickedButton.BackColor.R - 20, clickedButton.BackColor.G - 20, clickedButton.BackColor.B - 20); // Slightly darker
            }
        }

        /// <summary>
        /// Handles a click on a menu item button, adding it to the current order.
        /// </summary>
        private void HandleMenuItemClick(MenuItem item)
        {
            AddOrderItem(item);
        }

        /// <summary>
        /// Handles a double-click on an order item in the ListView to remove or reduce quantity.
        /// </summary>
        private void OrderListView_DoubleClick(object sender, EventArgs e)
        {
            if (orderListView.SelectedItems.Count > 0)
            {
                var selectedLvItem = orderListView.SelectedItems[0];
                var orderItem = selectedLvItem.Tag as OrderItem; // Retrieve the stored OrderItem object

                if (orderItem != null)
                {
                    // Confirm action with the user
                    var dialogResult = MessageBox.Show(
                        $"Do you want to remove all '{orderItem.ItemName}' from the order (Yes) or just reduce its quantity (No)?",
                        "Remove Item", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (dialogResult == DialogResult.Yes)
                    {
                        _currentOrder.OrderItems.Remove(orderItem);
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        if (orderItem.Quantity > 1)
                        {
                            orderItem.Quantity--;
                        }
                        else
                        {
                            _currentOrder.OrderItems.Remove(orderItem); // If quantity is 1, remove it
                        }
                    }
                    UpdateOrderSummary(); // Refresh UI
                }
            }
        }

        /// <summary>
        /// Handles change in the order type combo box, showing/hiding delivery info.
        /// </summary>
        private void OrderTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (orderTypeCombo.SelectedItem is OrderType selectedType)
            {
                _currentOrder.OrderType = selectedType;
                deliveryInfoPanel.Visible = (selectedType == OrderType.Delivery);
                statusLabel.Text = $"Status: New Order ({selectedType})";

                // Clear delivery customer info if switching from Delivery
                if (selectedType != OrderType.Delivery)
                {
                    customerNameBox.Clear();
                    customerPhoneBox.Clear();
                    deliveryAddressBox.Clear();
                    _currentOrder.CustomerId = null;
                    _currentOrder.Customer = null;
                }
            }
        }

        /// <summary>
        /// Handles the customer phone number field losing focus, attempting to look up customer.
        /// </summary>
        private async void CustomerPhoneBox_Leave(object sender, EventArgs e)
        {
            string phoneNumber = customerPhoneBox.Text.Trim();
            if (string.IsNullOrEmpty(phoneNumber)) return;

            try
            {
                var customer = await _orderController.GetCustomerAsync(phoneNumber);
                if (customer != null)
                {
                    _currentOrder.CustomerId = customer.Id;
                    _currentOrder.Customer = customer; // Attach for easy access
                    customerNameBox.Text = customer.Name;
                    deliveryAddressBox.Text = customer.Address;
                    MessageBox.Show("Customer found!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Customer not found. Please enter details for a new customer.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _currentOrder.CustomerId = null;
                    _currentOrder.Customer = null;
                    customerNameBox.Clear(); // Clear name/address so user can input new ones
                    deliveryAddressBox.Clear();
                    customerNameBox.Focus(); // Prompt for name input
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching for customer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Pay button click, processes payment, saves order, and prints receipt.
        /// </summary>
        private async void HandlePayButtonClick(object sender, EventArgs e)
        {
            if (!_currentOrder.OrderItems.Any())
            {
                MessageBox.Show("Cannot complete an empty order.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate delivery info if order type is Delivery
            if (_currentOrder.OrderType == OrderType.Delivery)
            {
                if (string.IsNullOrWhiteSpace(customerNameBox.Text) ||
                    string.IsNullOrWhiteSpace(customerPhoneBox.Text) ||
                    string.IsNullOrWhiteSpace(deliveryAddressBox.Text))
                {
                    MessageBox.Show("Please fill in all delivery information.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    Customer customer = await _orderController.GetCustomerAsync(customerPhoneBox.Text.Trim());
                    if (customer == null)
                    {
                        // Create new customer if not found
                        customer = new Customer
                        {
                            Name = customerNameBox.Text.Trim(),
                            Phone = customerPhoneBox.Text.Trim(), // Assuming phonetic is phone number
                            Address = deliveryAddressBox.Text.Trim(),
                            IsActive = true // New customers are active by default
                        };
                        customer = await _orderController.CreateCustomerAsync(customer);
                    }
                    else
                    {
                        // Update existing customer details if they've changed
                        customer.Name = customerNameBox.Text.Trim();
                        customer.Address = deliveryAddressBox.Text.Trim();
                        // Assuming UpdateCustomer is available in OrderController or a CustomerController
                        // await _orderController.UpdateCustomerAsync(customer); // Uncomment if you add this
                    }

                    _currentOrder.CustomerId = customer.Id;
                    _currentOrder.Customer = customer;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing customer information for delivery: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Open payment dialog
            using (var paymentForm = new PaymentForm(_currentOrder.Total))
            {
                if (paymentForm.ShowDialog() == DialogResult.OK)
                {
                    _currentOrder.AmountPaid = paymentForm.AmountPaid;
                    _currentOrder.Change = paymentForm.Change;
                    _currentOrder.OrderStatus = OrderStatus.Completed; // Mark order as complete
                    _currentOrder.OrderNumber = GenerateOrderNumber(); // Generate a unique order number

                    try
                    {
                        // Save the order to the database
                        await _orderController.CreateOrderAsync(_currentOrder);

                        MessageBox.Show($"Order {_currentOrder.OrderNumber} completed!\nAmount Paid: {_currentOrder.AmountPaid:N2}\nChange: {_currentOrder.Change:N2}",
                                        "Payment Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Print receipt if successful
                        _printService.PrintReceipt(_currentOrder);

                        InitializeNewOrder(); // Start a new order after completion
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _currentOrder.OrderStatus = OrderStatus.Pending; // Revert status on error
                    }
                }
            }
        }

        /// <summary>
        /// Sets the order type to Take Away via the combo box.
        /// </summary>
        private void HandleTakeAwayButtonClick(object sender, EventArgs e)
        {
            orderTypeCombo.SelectedItem = OrderType.Takeaway;
        }

        /// <summary>
        /// Sets the order type to Delivery via the combo box.
        /// </summary>
        private void HandleDeliveryButtonClick(object sender, EventArgs e)
        {
            orderTypeCombo.SelectedItem = OrderType.Delivery;
        }

        /// <summary>
        /// Handles the Admin button click, typically opening an admin panel.
        /// </summary>
        private void AdminButton_Click(object sender, EventArgs e)
        {
            // Example: Open an admin form. You would create AdminForm.cs
            MessageBox.Show("Opening Admin Panel...", "Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // using (var adminForm = Program.ServiceProvider.GetRequiredService<AdminForm>()) // If AdminForm is in DI
            // {
            //     adminForm.ShowDialog();
            // }
        }

        /// <summary>
        /// Handles the Reports button click, typically opening a reports panel.
        /// </summary>
        private void ReportsButton_Click(object sender, EventArgs e)
        {
            // Example: Open a reports form. You would create ReportsForm.cs
            MessageBox.Show("Opening Reports Panel...", "Reports", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // using (var reportsForm = Program.ServiceProvider.GetRequiredService<ReportsForm>()) // If ReportsForm is in DI
            // {
            //     reportsForm.ShowDialog();
            // }
        }

        // --- Helper Methods ---

        /// <summary>
        /// Generates a simple, unique order number based on current timestamp.
        /// For a real application, consider a more robust system (e.g., database sequence, UUID).
        /// </summary>
        /// <returns>A string representing the new order number.</returns>
        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now.ToString("yyMMddHHmmss")}";
        }
    }

    // --- Placeholder Classes/Enums (ensure these are in your POS.Models namespace or a dedicated Enums.cs) ---

    // --- Placeholder for PaymentForm (You'd create this as a separate WinForms Form) ---
    // This is included here for completeness of MainForm logic, but should be its own file.
    public partial class PaymentForm : Form
    {
        public decimal AmountPaid { get; private set; }
        public decimal Change { get; private set; }

        private decimal _orderTotal;
        private TextBox amountPaidTextBox;
        private Label totalDisplayLabel;
        private Label changeDisplayLabel;
        private Button confirmButton;

        public PaymentForm(decimal orderTotal)
        {
            _orderTotal = orderTotal;
            InitializePaymentFormComponents();
            totalDisplayLabel.Text = $"Order Total: ${_orderTotal:N2}";
            UpdateChange(); // Initialize change display
        }

        private void InitializePaymentFormComponents()
        {
            this.Text = "Process Payment";
            this.Size = new Size(350, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Non-resizable dialog
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            totalDisplayLabel = new Label
            {
                Text = $"Order Total: ${_orderTotal:N2}",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            var amountPaidLabel = new Label
            {
                Text = "Amount Paid:",
                Location = new Point(20, 70),
                AutoSize = true
            };

            amountPaidTextBox = new TextBox
            {
                Location = new Point(120, 67),
                Width = 150,
                Font = new Font("Arial", 12)
            };
            amountPaidTextBox.TextChanged += (s, e) => UpdateChange();
            amountPaidTextBox.KeyPress += AmountPaidTextBox_KeyPress; // Allow only numbers and decimal

            var changeLabel = new Label
            {
                Text = "Change:",
                Location = new Point(20, 120),
                AutoSize = true
            };

            changeDisplayLabel = new Label
            {
                Text = "$0.00",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(120, 117),
                AutoSize = true,
                ForeColor = Color.Blue
            };

            confirmButton = new Button
            {
                Text = "Confirm Payment",
                Location = new Point(100, 170),
                Size = new Size(150, 40),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            confirmButton.Click += ConfirmButton_Click;

            this.Controls.Add(totalDisplayLabel);
            this.Controls.Add(amountPaidLabel);
            this.Controls.Add(amountPaidTextBox);
            this.Controls.Add(changeLabel);
            this.Controls.Add(changeDisplayLabel);
            this.Controls.Add(confirmButton);
        }

        private void UpdateChange()
        {
            if (decimal.TryParse(amountPaidTextBox.Text, out decimal paid))
            {
                AmountPaid = paid;
                Change = paid - _orderTotal;
                changeDisplayLabel.Text = $"{Change:N2}";
                changeDisplayLabel.ForeColor = Change >= 0 ? Color.Blue : Color.Red;
                confirmButton.Enabled = paid >= _orderTotal; // Enable confirm only if enough paid
            }
            else
            {
                AmountPaid = 0;
                Change = -_orderTotal;
                changeDisplayLabel.Text = $"{-_orderTotal:N2}";
                changeDisplayLabel.ForeColor = Color.Red;
                confirmButton.Enabled = false;
            }
        }

        private void AmountPaidTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow numbers, backspace, and a single decimal point
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // Only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            if (AmountPaid >= _orderTotal)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Amount paid is less than the total.", "Payment Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}