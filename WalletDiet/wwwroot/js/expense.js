
document.addEventListener("DOMContentLoaded", function () {

    // 1. CURRENCY INPUT FORMATTING
    function setupCurrencyInput(displayId, valueId) {
        const displayInput = document.getElementById(displayId);
        const hiddenInput = document.getElementById(valueId);

        if (!displayInput || !hiddenInput) return;

        function formatCurrency(value) {
            const numbersOnly = value.replace(/\D/g, "");

            hiddenInput.value = numbersOnly;

            displayInput.value = numbersOnly
                ? new Intl.NumberFormat("id-ID").format(Number(numbersOnly))
                : "";
        }

        displayInput.addEventListener("input", function () {
            formatCurrency(this.value);
        });
    }

    setupCurrencyInput("expenseAmountDisplay", "expenseAmountValue");


    // 2. CUSTOM FORM VALIDATION
    const expenseForm = document.getElementById("expenseForm");

    if (!expenseForm) return;

    const titleInput = document.getElementById("expenseName");
    const amountDisplay = document.getElementById("expenseAmountDisplay");
    const amountValue = document.getElementById("expenseAmountValue");

    const titleError = document.getElementById("titleError");
    const amountError = document.getElementById("amountError");
    const categoryError = document.getElementById("categoryError");
    const deleteModal = document.getElementById("deleteExpenseModal");
    const deleteForms = document.querySelectorAll(".delete-expense-form");
    const cancelDelete = document.getElementById("cancelDeleteExpense");
    const confirmDelete = document.getElementById("confirmDeleteExpense");

    let selectedDeleteForm = null;

    const categoryInputs = document.querySelectorAll(
        'input[name="Category"]'
    );

    // Clear error messages
    function clearErrors() {
        if (titleError) titleError.textContent = "";
        if (amountError) amountError.textContent = "";
        if (categoryError) categoryError.textContent = "";
    }

    // Validate form when submitted
    expenseForm.addEventListener("submit", function (event) {
        event.preventDefault();

        clearErrors();

        let isValid = true;

        // Validate Expense Name
        if (titleInput.value.trim() === "") {
            titleError.textContent = "Expense name is required.";
            isValid = false;
        }

        // Validate Amount
        const amount = Number(amountValue.value);

        if (!amountValue.value || amount <= 0) {
            amountError.textContent = "Please enter a valid amount.";
            isValid = false;
        }

        // Validate Category
        const selectedCategory = document.querySelector(
            'input[name="Category"]:checked'
        );

        if (!selectedCategory) {
            categoryError.textContent = "Please select a category.";
            isValid = false;
        }

        // Submit form if all fields are valid
        if (isValid) {
            expenseForm.submit();
        }
    });


    // 3. CLEAR ERRORS WHEN USER EDITS INPUTS
    titleInput.addEventListener("input", function () {
        titleError.textContent = "";
    });

    amountDisplay.addEventListener("input", function () {
        amountError.textContent = "";
    });

    categoryInputs.forEach(function (category) {
        category.addEventListener("change", function () {
            categoryError.textContent = "";
        });
    });

    deleteForms.forEach(function (form) {
        form.addEventListener("submit", function (event) {
            event.preventDefault();

            selectedDeleteForm = form;
            deleteModal.classList.add("active");
        });
    });

    cancelDelete.addEventListener("click", function () {
        deleteModal.classList.remove("active");
        selectedDeleteForm = null;
    });

    confirmDelete.addEventListener("click", function () {
        if (selectedDeleteForm) {
            selectedDeleteForm.submit();
        }
    });

    deleteModal.addEventListener("click", function (event) {
        if (event.target === deleteModal) {
            deleteModal.classList.remove("active");
            selectedDeleteForm = null;
        }
    });

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" && deleteModal.classList.contains("active")) {
            deleteModal.classList.remove("active");
            selectedDeleteForm = null;
        }
    });

});
