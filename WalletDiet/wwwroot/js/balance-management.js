document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // 1. ADD BALANCE MODAL
    // ==========================================

    const addBalanceBtn = document.getElementById("addBalanceBtn");
    const addBalanceModal = document.getElementById("addBalanceModal");
    const cancelAddBalance = document.getElementById("cancelAddBalance");

    function openAddBalanceModal() {
        addBalanceModal?.classList.add("active");
    }

    function closeAddBalanceModal() {
        addBalanceModal?.classList.remove("active");
    }

    addBalanceBtn?.addEventListener("click", openAddBalanceModal);
    cancelAddBalance?.addEventListener("click", closeAddBalanceModal);

    addBalanceModal?.addEventListener("click", function (event) {
        if (event.target === addBalanceModal) {
            closeAddBalanceModal();
        }
    });


    // ==========================================
    // 2. REMOVE BALANCE MODAL
    // ==========================================

    const removeBalanceBtn = document.getElementById("removeBalanceBtn");
    const removeBalanceModal = document.getElementById("removeBalanceModal");
    const cancelRemoveBalance = document.getElementById("cancelRemoveBalance");

    function openRemoveBalanceModal() {
        removeBalanceModal?.classList.add("active");
    }

    function closeRemoveBalanceModal() {
        removeBalanceModal?.classList.remove("active");
    }

    removeBalanceBtn?.addEventListener("click", openRemoveBalanceModal);
    cancelRemoveBalance?.addEventListener("click", closeRemoveBalanceModal);

    removeBalanceModal?.addEventListener("click", function (event) {
        if (event.target === removeBalanceModal) {
            closeRemoveBalanceModal();
        }
    });

    // ==========================================
    // 3. CLOSE MODAL WITH ESC KEY
    // ==========================================

    document.addEventListener("keydown", function (event) {

        if (event.key === "Escape") {
            closeAddBalanceModal();
            closeRemoveBalanceModal();
        }

    });

    // ==========================================
    // 4. CURRENCY INPUT FORMATTING
    // ==========================================

    function setupCurrencyInput(displayId, valueId) {

        const displayInput = document.getElementById(displayId);
        const hiddenInput = document.getElementById(valueId);

        if (!displayInput || !hiddenInput) {
            return;
        }

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

        const form = displayInput.closest("form");

        if (form) {
            form.addEventListener("submit", function () {
                hiddenInput.value = displayInput.value.replace(/\D/g, "");
            });
        }
    }

    setupCurrencyInput("addAmountDisplay", "addAmountValue");
    setupCurrencyInput("removeAmountDisplay", "removeAmountValue");


});
