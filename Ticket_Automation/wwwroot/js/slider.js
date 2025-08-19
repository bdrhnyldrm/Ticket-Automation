document.addEventListener("DOMContentLoaded", function () {
    const slides = document.querySelectorAll(".slider-content");
    const dots = document.querySelectorAll(".slider-dots span");

    let currentIndex = 0;

    function showSlide(index) {
        slides.forEach((slide, i) => {
            slide.classList.toggle("active", i === index);
            dots[i].classList.toggle("active", i === index);
        });
    }

    dots.forEach((dot, index) => {
        dot.addEventListener("click", () => {
            currentIndex = index;
            showSlide(currentIndex);
        });
    });

    setInterval(() => {
        currentIndex = (currentIndex + 1) % slides.length;
        showSlide(currentIndex);
    }, 4000); // 4 saniyede bir geçiş

    showSlide(currentIndex);
});
