document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // 1. CURRENCY INPUT FORMATTING
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

        // Format initial value when page loads
        if (hiddenInput.value) {
            formatCurrency(hiddenInput.value);
        }

        // Format while typing
        displayInput.addEventListener("input", function () {
            formatCurrency(this.value);
        });
    }

    setupCurrencyInput(
        "initialBalanceDisplay",
        "initialBalanceValue"
    );

    setupCurrencyInput(
        "customAllowanceDisplay",
        "customAllowanceValue"
    );


    // ==========================================
    // 2. DAILY ALLOWANCE MODE
    // ==========================================

    const recommendedOption =
        document.getElementById("recommendedOption");

    const customOption =
        document.getElementById("customOption");

    const customAllowanceGroup =
        document.getElementById("customAllowanceGroup");

    const customAllowanceDisplay =
        document.getElementById("customAllowanceDisplay");

    const customAllowanceValue =
        document.getElementById("customAllowanceValue");


    function updateAllowanceMode() {

        if (!customAllowanceGroup ||
            !recommendedOption ||
            !customOption) {
            return;
        }

        const isCustom = customOption.checked;

        customAllowanceGroup.classList.toggle(
            "is-visible",
            isCustom
        );

        if (!isCustom) {

            if (customAllowanceDisplay) {
                customAllowanceDisplay.value = "";
            }

            if (customAllowanceValue) {
                customAllowanceValue.value = "";
            }
        }
    }

    if (recommendedOption && customOption) {

        recommendedOption.addEventListener(
            "change",
            updateAllowanceMode
        );

        customOption.addEventListener(
            "change",
            updateAllowanceMode
        );

        updateAllowanceMode();
    }


    // ==========================================
    // 3. DATE VALIDATION
    // ==========================================

    const startDateInput =
        document.getElementById("startDateInput");

    const targetDateInput =
        document.getElementById("targetDateInput");

    function updateTargetDateMinimum() {

        if (!startDateInput || !targetDateInput) {
            return;
        }

        const startDate = startDateInput.value;

        targetDateInput.min = startDate;

        if (targetDateInput.value &&
            targetDateInput.value < startDate) {

            targetDateInput.value = startDate;
        }
    }

    if (startDateInput && targetDateInput) {

        startDateInput.addEventListener(
            "change",
            updateTargetDateMinimum
        );

        updateTargetDateMinimum();
    }


    // ==========================================
    // 4. FORM SUBMISSION
    // ==========================================

    const createBudgetForm =
        document.getElementById("createBudgetForm");

    if (createBudgetForm) {

        createBudgetForm.addEventListener("submit", function () {

            // Ensure currency values are synchronized
            const initialDisplay =
                document.getElementById("initialBalanceDisplay");

            const initialValue =
                document.getElementById("initialBalanceValue");

            const customDisplay =
                document.getElementById("customAllowanceDisplay");

            const customValue =
                document.getElementById("customAllowanceValue");

            if (initialDisplay && initialValue) {

                initialValue.value =
                    initialDisplay.value.replace(/\D/g, "");
            }

            if (customDisplay && customValue) {

                customValue.value =
                    customDisplay.value.replace(/\D/g, "");
            }

        });
    }

});