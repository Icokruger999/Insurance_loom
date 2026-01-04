// Application Page JavaScript

// API Base URL
const API_BASE_URL = (() => {
    const hostname = window.location.hostname;
    
    // Production (AWS)
    if (hostname === 'insuranceloom.com' || hostname === 'www.insuranceloom.com') {
        return 'https://api.insuranceloom.com/api';
    }
    
    // Development (localhost)
    return 'http://localhost:5000/api';
})();

let currentStep = 1;
const totalSteps = 4;
let selectedServiceId = null;
let selectedServiceName = null;

// Initialize application page
document.addEventListener('DOMContentLoaded', () => {
    // Get service ID and name from URL parameters or localStorage
    const urlParams = new URLSearchParams(window.location.search);
    selectedServiceId = urlParams.get('serviceId') || localStorage.getItem('selectedServiceId');
    selectedServiceName = urlParams.get('serviceName') || localStorage.getItem('selectedServiceName');
    
    if (selectedServiceName) {
        document.getElementById('serviceName').textContent = selectedServiceName;
    }
    
    // Clear localStorage after reading
    localStorage.removeItem('selectedServiceId');
    localStorage.removeItem('selectedServiceName');
});

function nextStep() {
    // Validate current step
    if (!validateCurrentStep()) {
        return;
    }
    
    if (currentStep < totalSteps) {
        // Update step indicator
        updateStepIndicator(currentStep + 1);
        
        // Hide current step
        document.getElementById(`step${currentStep}`).classList.remove('active');
        
        // Show next step
        currentStep++;
        document.getElementById(`step${currentStep}`).classList.add('active');
        
        // If on review step, populate review summary
        if (currentStep === 4) {
            populateReviewSummary();
        }
        
        // Scroll to top
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
}

function previousStep() {
    if (currentStep > 1) {
        // Update step indicator
        updateStepIndicator(currentStep - 1);
        
        // Hide current step
        document.getElementById(`step${currentStep}`).classList.remove('active');
        
        // Show previous step
        currentStep--;
        document.getElementById(`step${currentStep}`).classList.add('active');
        
        // Scroll to top
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
}

function updateStepIndicator(step) {
    const stepItems = document.querySelectorAll('.step-item');
    const stepConnectors = document.querySelectorAll('.step-connector');
    
    stepItems.forEach((item, index) => {
        const stepNum = index + 1;
        item.classList.remove('active', 'completed');
        
        if (stepNum < step) {
            item.classList.add('completed');
            if (stepConnectors[index - 1]) {
                stepConnectors[index - 1].classList.add('completed');
            }
        } else if (stepNum === step) {
            item.classList.add('active');
        }
    });
}

function validateCurrentStep() {
    const currentStepEl = document.getElementById(`step${currentStep}`);
    const requiredFields = currentStepEl.querySelectorAll('[required]');
    let isValid = true;
    
    requiredFields.forEach(field => {
        if (!field.value.trim()) {
            isValid = false;
            field.style.borderColor = 'var(--danger-color)';
        } else {
            field.style.borderColor = '';
        }
    });
    
    if (!isValid) {
        showError('Please fill in all required fields before proceeding.');
    }
    
    return isValid;
}

function populateReviewSummary() {
    const summary = document.getElementById('reviewSummary');
    
    const formData = {
        firstName: document.getElementById('firstName').value,
        lastName: document.getElementById('lastName').value,
        idNumber: document.getElementById('idNumber').value,
        dateOfBirth: document.getElementById('dateOfBirth').value,
        email: document.getElementById('email').value,
        phone: document.getElementById('phone').value,
        streetName: document.getElementById('streetName').value,
        suburb: document.getElementById('suburb').value,
        region: document.getElementById('region').value,
        monthlyIncome: document.getElementById('monthlyIncome').value,
        preferredPaymentDate: document.getElementById('preferredPaymentDate').value
    };
    
    summary.innerHTML = `
        <h3 style="margin-bottom: 1.5rem; color: var(--primary-color);">Application Summary</h3>
        <div style="display: grid; gap: 1rem;">
            <div><strong>Service:</strong> ${selectedServiceName || 'Not specified'}</div>
            <div><strong>Name:</strong> ${formData.firstName} ${formData.lastName}</div>
            <div><strong>ID Number:</strong> ${formData.idNumber}</div>
            <div><strong>Date of Birth:</strong> ${formData.dateOfBirth}</div>
            <div><strong>Email:</strong> ${formData.email}</div>
            <div><strong>Phone:</strong> ${formData.phone}</div>
            <div><strong>Street:</strong> ${formData.streetName}</div>
            <div><strong>Suburb:</strong> ${formData.suburb}</div>
            <div><strong>Region:</strong> ${formData.region}</div>
            ${formData.monthlyIncome ? `<div><strong>Monthly Income:</strong> R${parseFloat(formData.monthlyIncome).toLocaleString()}</div>` : ''}
            ${formData.preferredPaymentDate ? `<div><strong>Preferred Payment Date:</strong> ${formData.preferredPaymentDate} of each month</div>` : ''}
        </div>
    `;
}

async function submitApplication() {
    const errorDiv = document.getElementById('applicationError');
    errorDiv.classList.remove('show');
    
    // Validate all steps
    if (!validateCurrentStep()) {
        return;
    }
    
    // Submit application anonymously (no account required)
    try {
        const formData = collectFormData();
        
        const response = await fetch(`${API_BASE_URL}/client/application`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                ...formData,
                serviceTypeId: selectedServiceId
            })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            // Success - show success message and redirect
            showSuccessMessage('Application submitted successfully! A broker will review your application and contact you shortly.');
            
            // Redirect to home page after 3 seconds
            setTimeout(() => {
                window.location.href = '/';
            }, 3000);
        } else {
            showError(data.message || 'Failed to submit application. Please try again.');
        }
    } catch (error) {
        console.error('Application submission error:', error);
        showError('Connection error. Please check your internet connection and try again.');
    }
}

function collectFormData() {
    return {
        firstName: document.getElementById('firstName').value,
        lastName: document.getElementById('lastName').value,
        idNumber: document.getElementById('idNumber').value,
        dateOfBirth: document.getElementById('dateOfBirth').value,
        email: document.getElementById('email').value,
        phone: document.getElementById('phone').value,
        streetName: document.getElementById('streetName').value,
        suburb: document.getElementById('suburb').value,
        region: document.getElementById('region').value,
        monthlyIncome: document.getElementById('monthlyIncome').value ? parseFloat(document.getElementById('monthlyIncome').value) : null,
        paymentDate: document.getElementById('preferredPaymentDate').value ? parseInt(document.getElementById('preferredPaymentDate').value) : null
    };
}

function showError(message) {
    const errorDiv = document.getElementById('applicationError');
    errorDiv.textContent = message;
    errorDiv.classList.add('show');
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function showSuccessMessage(message) {
    // Create or update success message element
    let successDiv = document.getElementById('applicationSuccess');
    if (!successDiv) {
        successDiv = document.createElement('div');
        successDiv.id = 'applicationSuccess';
        successDiv.className = 'success-message';
        successDiv.style.cssText = 'display: block; color: #065f46; font-size: 0.875rem; margin-top: 0.5rem; padding: 0.75rem; background-color: #d1fae5; border-radius: 6px; border: 1px solid #6ee7b7; margin-bottom: 1rem;';
        const errorDiv = document.getElementById('applicationError');
        errorDiv.parentNode.insertBefore(successDiv, errorDiv);
    }
    successDiv.textContent = message;
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function showSuccessMessageHTML(htmlContent) {
    // Create or update success message element with HTML content
    let successDiv = document.getElementById('applicationSuccess');
    if (!successDiv) {
        successDiv = document.createElement('div');
        successDiv.id = 'applicationSuccess';
        successDiv.className = 'success-message';
        successDiv.style.cssText = 'display: block; margin-top: 0.5rem; padding: 1.5rem; background-color: #d1fae5; border-radius: 6px; border: 1px solid #6ee7b7; margin-bottom: 1rem;';
        const errorDiv = document.getElementById('applicationError');
        errorDiv.parentNode.insertBefore(successDiv, errorDiv);
    }
    successDiv.innerHTML = htmlContent;
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

