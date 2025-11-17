// Time Slot Selection
document.addEventListener('DOMContentLoaded', function() {
    const timeSlots = document.querySelectorAll('.time-slot');
    
    timeSlots.forEach(slot => {
        slot.addEventListener('click', function() {
            // Remove selected class from all slots
            timeSlots.forEach(s => s.classList.remove('selected'));
            
            // Add selected class to clicked slot
            this.classList.add('selected');
            
            // Update hidden input if exists
            const timeInput = document.getElementById('SelectedTime');
            if (timeInput) {
                timeInput.value = this.dataset.time;
            }
        });
    });
});

// Form Validation Enhancement
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (form) {
        form.addEventListener('submit', function(e) {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    }
}

// Toast Notifications
function showToast(message, type = 'info') {
    const toastHtml = `
        <div class="toast align-items-center text-white bg-${type} border-0 position-fixed top-0 end-0 m-3" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', toastHtml);
    const toastElement = document.querySelector('.toast:last-child');
    const toast = new bootstrap.Toast(toastElement);
    toast.show();
    
    toastElement.addEventListener('hidden.bs.toast', function () {
        toastElement.remove();
    });
}

// Confirm Delete
function confirmDelete(message) {
    return confirm(message || 'Silmək istədiyinizə əminsiniz?');
}

// Date Picker Enhancement (if needed)
function initializeDatePickers() {
    const dateInputs = document.querySelectorAll('input[type="date"]');
    dateInputs.forEach(input => {
        // Set min date to today
        const today = new Date().toISOString().split('T')[0];
        if (!input.hasAttribute('min')) {
            input.setAttribute('min', today);
        }
    });
}

// Initialize on load
document.addEventListener('DOMContentLoaded', function() {
    initializeDatePickers();
});
