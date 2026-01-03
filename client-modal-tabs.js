// Client Modal Tab Functionality
document.addEventListener('DOMContentLoaded', () => {
    const clientModal = document.getElementById('clientModal');
    if (!clientModal) return;
    
    const tabButtons = clientModal.querySelectorAll('.tab-button');
    const tabContents = clientModal.querySelectorAll('.tab-content');
    
    tabButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            const targetTab = btn.dataset.tab;
            
            // Update active tab button
            tabButtons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            
            // Update active tab content
            tabContents.forEach(c => c.classList.remove('active'));
            const targetContent = clientModal.querySelector(`#${targetTab}`);
            if (targetContent) {
                targetContent.classList.add('active');
            }
        });
    });
});

