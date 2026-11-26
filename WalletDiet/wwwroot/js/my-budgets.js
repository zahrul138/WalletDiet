document.addEventListener("DOMContentLoaded", function () {

    // =========================================
    // ELEMENT REFERENCES
    // =========================================

    const filterButtons = document.querySelectorAll(".filter-btn");
    const budgetCards = document.querySelectorAll(".budget-card");
    const filterEmpty = document.getElementById("filterEmpty");

    const deleteButtons = document.querySelectorAll(".budget-delete-btn");
    const deleteModal = document.getElementById("deleteModal");
    const cancelDelete = document.getElementById("cancelDelete");
    const confirmDelete = document.getElementById("confirmDelete");

    let pendingDeleteForm = null;


    // =========================================
    // UPDATE FILTER COUNTS
    // =========================================

    ["active", "upcoming", "completed"].forEach(function (status) {

        const count = document.querySelectorAll(
            `.budget-card[data-status="${status}"]`
        ).length;

        const button = document.querySelector(
            `.filter-btn[data-filter="${status}"]`
        );

        if (button) {
            button.insertAdjacentHTML(
                "beforeend",
                `<span class="filter-count">${count}</span>`
            );
        }

    });


    // =========================================
    // FILTER BUDGETS
    // =========================================

    filterButtons.forEach(function (button) {

        button.addEventListener("click", function () {

            const selectedFilter = this.dataset.filter;
            let visibleCount = 0;

            filterButtons.forEach(function (item) {
                item.classList.remove("active");
            });

            this.classList.add("active");

            budgetCards.forEach(function (card) {

                const status = card.dataset.status;

                const shouldShow =
                    selectedFilter === "all" ||
                    selectedFilter === status;

                card.hidden = !shouldShow;

                if (shouldShow) {
                    visibleCount++;
                }

            });

            if (filterEmpty) {
                filterEmpty.hidden =
                    budgetCards.length === 0 || visibleCount > 0;
            }

        });

    });


    // =========================================
    // CHECK DELETE MODAL ELEMENTS
    // =========================================

    if (!deleteModal || !cancelDelete || !confirmDelete) {
        console.error("Delete modal elements could not be found.");
        return;
    }


    // =========================================
    // OPEN DELETE MODAL
    // =========================================

    deleteButtons.forEach(function (button) {

        button.addEventListener("click", function () {

            const form = button.closest(".delete-budget-form");

            if (!form) {
                console.error("Delete form could not be found.");
                return;
            }

            pendingDeleteForm = form;

            deleteModal.hidden = false;
            deleteModal.classList.add("is-visible");
            deleteModal.setAttribute("aria-hidden", "false");

        });

    });


    // =========================================
    // CLOSE DELETE MODAL
    // =========================================

    function closeDeleteModal() {

        deleteModal.classList.remove("is-visible");
        deleteModal.setAttribute("aria-hidden", "true");
        deleteModal.hidden = true;

        pendingDeleteForm = null;

    }


    // =========================================
    // CANCEL DELETE
    // =========================================

    cancelDelete.addEventListener("click", function () {
        closeDeleteModal();
    });


    // =========================================
    // CONFIRM DELETE
    // =========================================

    confirmDelete.addEventListener("click", function () {

        if (!pendingDeleteForm) {
            console.error("No budget form selected for deletion.");
            return;
        }

        const formToDelete = pendingDeleteForm;

        closeDeleteModal();

        HTMLFormElement.prototype.submit.call(formToDelete);

    });


    // =========================================
    // CLICK OUTSIDE MODAL
    // =========================================

    deleteModal.addEventListener("click", function (event) {

        if (event.target === deleteModal) {
            closeDeleteModal();
        }

    });


    // =========================================
    // ESCAPE KEY
    // =========================================

    document.addEventListener("keydown", function (event) {

        if (
            event.key === "Escape" &&
            deleteModal.classList.contains("is-visible")
        ) {
            closeDeleteModal();
        }

    });


    console.log("MyBudgets JavaScript initialized.");

});
