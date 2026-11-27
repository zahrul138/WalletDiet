document.addEventListener("DOMContentLoaded", function () {

    const deleteModal = document.getElementById("deleteExpenseModal");
    const deleteForms = document.querySelectorAll(".delete-expense-form");
    const cancelDelete = document.getElementById("cancelDeleteExpense");
    const confirmDelete = document.getElementById("confirmDeleteExpense");

    let selectedDeleteForm = null;

    if (!deleteModal || !cancelDelete || !confirmDelete) {
        return;
    }

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
