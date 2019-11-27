window.Preloader = {
    show: function () {
        var htmlElement = this.getElement();
        if (!htmlElement) {
            document.body.insertAdjacentHTML('beforeend', '<div class="preloader" id="preloader-global"></div>');
        }
    },
    hide: function () {
        var htmlElement = this.getElement();
        if (htmlElement) {
            htmlElement.remove();
        }
    },
    getElement: function () {
        return document.getElementById("preloader-global");
    }
} 