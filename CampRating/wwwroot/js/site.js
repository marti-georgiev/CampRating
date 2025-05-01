// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Star rating functionality
document.addEventListener('DOMContentLoaded', function() {
    const ratingSelect = document.querySelector('select[name="NewReview.Rating"]');
    if (ratingSelect) {
        ratingSelect.addEventListener('change', function() {
            const selectedValue = this.value;
            const starsContainer = this.closest('.form-group').querySelector('.stars');
            if (starsContainer) {
                starsContainer.innerHTML = '';
                for (let i = 1; i <= 5; i++) {
                    const star = document.createElement('span');
                    star.className = `fa fa-star ${i <= selectedValue ? 'text-warning' : 'text-muted'}`;
                    starsContainer.appendChild(star);
                }
            }
        });
    }
}); 