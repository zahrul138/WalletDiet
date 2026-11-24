// Password
const passwordForm = document.getElementById("password-form");
const currentPasswordInput = document.getElementById("current-password");
const passwordInput = document.getElementById("profile-password");
const confirmPasswordInput = document.getElementById("confirm-password");

const editPassword = document.getElementById("edit-password");
const passwordEditIcon = document.getElementById("password-edit-icon");

// Password visibility buttons
const toggleCurrentPassword = document.getElementById("toggle-current-password");
const toggleNewPassword = document.getElementById("toggle-new-password");
const toggleConfirmPassword = document.getElementById("toggle-confirm-password");

// Username
const usernameDisplay = document.getElementById("username-display");
const usernameForm = document.getElementById("username-form");
const usernameInput = document.getElementById("username-input");
const editUsername = document.getElementById("edit-username");

// Email
const emailForm = document.getElementById("email-form");
const emailInput = document.getElementById("profile-email");
const editEmail = document.getElementById("edit-email");
const emailEditIcon = document.getElementById("email-edit-icon");

// Profile Picture
const editPicture = document.getElementById("edit-picture");
const pictureInput = document.getElementById("picture-input");
const picturePreview = document.getElementById("profile-picture-preview");

const pictureActions = document.getElementById("picture-actions");
const savePicture = document.getElementById("save-picture");
const cancelPicture = document.getElementById("cancel-picture");

const removePicture = document.getElementById("remove-picture");
const removePictureForm = document.getElementById("remove-picture-form");


// State
let isEditing = false;
let originalUsername = "";

let isEditingEmail = false;
let originalEmail = "";

let isEditingPassword = false;

let originalPicture = picturePreview.src;
let temporaryPictureUrl = null;


editUsername.addEventListener("click", function () {

    isEditing = true;
    originalUsername = usernameInput.value;

    usernameDisplay.classList.add("d-none");
    usernameInput.classList.remove("d-none");

    usernameInput.readOnly = false;
    usernameInput.focus();
    usernameInput.select();

});

usernameInput.addEventListener("keydown", function (e) {

    if (e.key === "Enter") {
        e.preventDefault();
        usernameInput.blur();
    }

});

usernameInput.addEventListener("blur", function () {

    if (!isEditing) return;

    isEditing = false;
    usernameInput.readOnly = true;

    usernameInput.classList.add("d-none");
    usernameDisplay.classList.remove("d-none");

    if (usernameInput.value != originalUsername) {
        usernameForm.requestSubmit();
    }

});

editEmail.addEventListener("click", function () {

    // Masuk mode edit
    if (!isEditingEmail) {

        isEditingEmail = true;
        originalEmail = emailInput.value;

        // Mode edit
        emailInput.disabled = false;
        emailInput.focus();

        editEmail.setAttribute("aria-label", "Save email");
        editEmail.title = "Save email";

        // Icon centang
        emailEditIcon.innerHTML = `
            <path d="m5 12 4 4L19 6" />
        `;

    }

    // Konfirmasi perubahan
    else {

        if (emailInput.value.trim() === originalEmail) {
            emailInput.readOnly = true;
            isEditingEmail = false;

            resetEmailIcon();
            return;
        }

        emailForm.requestSubmit();
    }

});

function resetEmailIcon() {

    emailEditIcon.innerHTML = `
        <path d="M12 20h9" />
        <path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L8 18l-4 1 1-4Z" />
    `;

    editEmail.setAttribute("aria-label", "Edit email");
    editEmail.title = "Edit email";
}

if (emailForm.dataset.editing === "true") {

    isEditingEmail = true;

    emailInput.disabled = false;
    emailInput.focus();

    editEmail.setAttribute("aria-label", "Save email");
    editEmail.title = "Save email";

    emailEditIcon.innerHTML = `
        <path d="m5 12 4 4L19 6" />
    `;
}


function setPasswordEditMode(enabled) {

    currentPasswordInput.disabled = !enabled;
    passwordInput.disabled = !enabled;
    confirmPasswordInput.disabled = !enabled;

    toggleCurrentPassword.disabled = !enabled;
    toggleNewPassword.disabled = !enabled;
    toggleConfirmPassword.disabled = !enabled;

}

// Mengubah ikon menjadi centang
function setPasswordSaveIcon() {

    passwordEditIcon.innerHTML = `
        <path d="m5 12 4 4L19 6" />
    `;

    editPassword.setAttribute("aria-label", "Save password");
    editPassword.title = "Save password";

}

// Mengembalikan ikon menjadi pensil
function resetPasswordIcon() {

    passwordEditIcon.innerHTML = `
        <path d="M12 20h9" />
        <path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L8 18l-4 1 1-4Z" />
    `;

    editPassword.setAttribute("aria-label", "Edit password");
    editPassword.title = "Edit password";

}

// Tombol edit password
editPassword.addEventListener("click", function () {

    if (!isEditingPassword) {

        isEditingPassword = true;

        setPasswordEditMode(true);
        setPasswordSaveIcon();

        currentPasswordInput.focus();

    } else {

        if (
            currentPasswordInput.value.trim() === "" ||
            passwordInput.value.trim() === "" ||
            confirmPasswordInput.value.trim() === ""
        ) {
            return;
        }

        passwordForm.requestSubmit();

    }

});

// Mengirim form ketika tombol Enter ditekan
[currentPasswordInput, passwordInput, confirmPasswordInput].forEach(input => {

    input.addEventListener("keydown", function (e) {

        if (e.key === "Enter") {

            e.preventDefault();

            if (!isEditingPassword) return;

            if (
                currentPasswordInput.value.trim() === "" ||
                passwordInput.value.trim() === "" ||
                confirmPasswordInput.value.trim() === ""
            ) {
                return;
            }

            passwordForm.requestSubmit();

        }

    });

});

// Mempertahankan mode edit jika validasi gagal
if (passwordForm.dataset.editing === "true") {

    isEditingPassword = true;

    setPasswordEditMode(true);
    setPasswordSaveIcon();

}

// Menampilkan password selama tombol mata ditekan
function holdPasswordVisibility(button, input) {

    function showPassword() {
        if (button.disabled) return;

        input.type = "text";
    }

    function hidePassword() {
        input.type = "password";
    }

    button.addEventListener("pointerdown", function (e) {

        e.preventDefault();

        showPassword();

        button.setPointerCapture(e.pointerId);

    });

    button.addEventListener("pointerup", hidePassword);
    button.addEventListener("pointercancel", hidePassword);
    button.addEventListener("lostpointercapture", hidePassword);

    button.addEventListener("contextmenu", function (e) {
        e.preventDefault();
    });

    window.addEventListener("blur", hidePassword);
}

holdPasswordVisibility(toggleCurrentPassword, currentPasswordInput);
holdPasswordVisibility(toggleNewPassword, passwordInput);
holdPasswordVisibility(toggleConfirmPassword, confirmPasswordInput);

editPicture.addEventListener("click", function () {

    // Jika user sudah memiliki foto sendiri
    if (removePicture) {

        // Tampilkan atau sembunyikan tombol Remove
        removePicture.classList.toggle("d-none");

    }
    else {

        // Jika masih menggunakan avatar default,
        // langsung buka File Explorer
        pictureInput.click();

    }

});


// Memilih gambar dan menampilkan preview
pictureInput.addEventListener("change", function () {

    const file = pictureInput.files[0];

    if (!file) return;

    // Membersihkan preview sementara sebelumnya
    if (temporaryPictureUrl) {
        URL.revokeObjectURL(temporaryPictureUrl);
    }

    temporaryPictureUrl = URL.createObjectURL(file);

    picturePreview.src = temporaryPictureUrl;

    pictureActions.classList.remove("d-none");

});


cancelPicture.addEventListener("click", function () {

    picturePreview.src = originalPicture;

    pictureInput.value = "";

    pictureActions.classList.add("d-none");

    if (temporaryPictureUrl) {
        URL.revokeObjectURL(temporaryPictureUrl);
        temporaryPictureUrl = null;
    }

});


savePicture.addEventListener("click", function () {

});

if (removePicture && removePictureForm) {

    removePicture.addEventListener("click", function () {

        

    });

}
