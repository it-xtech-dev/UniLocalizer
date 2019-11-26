window.VuePrompt = new Vue({
    name: 'Prompt',
    el: '[vue-instance="prompt"]',
    // original template use https://codepen.io/xtech-dev/pen/ExxrJNw?editors=1010 to compile it render function.
    //<transition name="fade">
    //    <div class="modal-back" v-if="isVisible">
    //        <div class="modal-dialog modal-dialog-centered" role="document">
    //            <div class="modal-content">
    //                <div class="modal-header">
    //                    <h5 class="modal-title" id="exampleModalCenterTitle" v-html="title"></h5>
    //                    <button type="button" class="close" data-dismiss="modal" aria-label="Close" v-on:click.prevent="hide('cancel')">
    //                        <span aria-hidden="true"><span class="close-icon"></span</span>
    //                    </button>
    //                </div>
    //                <div class="modal-body" v-html="body">
    //                </div>
    //                <div class="modal-footer">
    //                    <button v-for="btn in buttons"
    //                            type="button"
    //                            class="btn"
    //                            v-bind:class="btn.cssClass || 'btn-default'"
    //                            v-html="btn.text"
    //                            v-on:click.prevent="hide(btn.reason)"
    //                            data-dismiss="modal">
    //                    </button>
    //                </div>
    //            </div>
    //        </div>
    //    </div>
    //</transition>
    render: function () { with (this) { return _c('transition', { attrs: { "name": "fade" } }, [(isVisible) ? _c('div', { staticClass: "modal-back" }, [_c('div', { staticClass: "modal-dialog modal-dialog-centered", attrs: { "role": "document" } }, [_c('div', { staticClass: "modal-content" }, [_c('div', { staticClass: "modal-header" }, [_c('h5', { staticClass: "modal-title", attrs: { "id": "exampleModalCenterTitle" }, domProps: { "innerHTML": _s(title) } }), _v(" "), _c('button', { staticClass: "close", attrs: { "type": "button", "data-dismiss": "modal", "aria-label": "Close" }, on: { "click": function ($event) { $event.preventDefault(); return hide('cancel') } } }, [_c('span', { attrs: { "aria-hidden": "true" } }, [_c('span', { staticClass: "close-icon" })])])]), _v(" "), _c('div', { staticClass: "modal-body", domProps: { "innerHTML": _s(body) } }), _v(" "), _c('div', { staticClass: "modal-footer" }, _l((buttons), function (btn) { return _c('button', { staticClass: "btn", class: btn.cssClass || 'btn-default', attrs: { "type": "button", "data-dismiss": "modal" }, domProps: { "innerHTML": _s(btn.text) }, on: { "click": function ($event) { $event.preventDefault(); return hide(btn.reason) } } }) }), 0)])])]) : _e()]) } },
    data: {
        isVisible: false,
        title: null,
        body: null,
        buttons: null,
        onHide: null
    },
    methods: {
        show: function (config) {
            var self = this;
            this.title = config.title || 'Please confirm';
            this.body = config.body || 'Are your sure your want to perform this operation?';
            this.buttons = config.buttons || [
                { text: 'Yes', cssClass: 'btn-danger', reason: 'yes' },
                { text: 'No', cssClass: 'btn-default', reason: 'no' }
            ];
            this.onHide = config.onHide || this.onHide;

            // wait with hide till the next tick (required when next prompt will be called in hide callback)
            Vue.nextTick(function () {
                self.isVisible = true;
            });
        },
        hide: function (reason) {
            var onHide = this.onHide;
            this.isVisible = false;
            // reset to defaults
            this.body = null;
            this.title = null;
            this.onHide = null;
            this.buttons = null;

            if (typeof onHide === 'function') {
                onHide(reason);
            }
        },
        error: function (message) {
            this.title = '<span class="text-danger">An Error occured...</span>';
            this.body = message + "<br/><br/>If the problem is persistent please contact application administrator.";
            this.buttons = [{ text: 'Close', cssClass: 'btn-default', reason: 'cancel' }];
            this.isVisible = true;
        }
    },
    watch: {
        'isVisible': function (newValue, oldValue) {
            if (newValue !== oldValue) {
                if (newValue === true) {
                    document.body.classList.add("modal-open");
                } else {
                    document.body.classList.remove("modal-open");
                }
            }
        }
    }
});