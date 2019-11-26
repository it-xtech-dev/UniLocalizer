/* Vue select component
 * Requires http://www.malot.fr/bootstrap-datetimepicker
 * Copyright: it.xtech.pl
 */
Vue.component('v-select', {
    template: '<select v-model="model.selected" :class="model.cssClass"><option v-for="option in model.options" v-bind:value="option">{{ getText(option) }}</option></select>',
    props: ['model'], /* expected format: 
     * { 
     *      options : [], // list of options for select (can be list of objects)
     *      selected : {}, // object 
     *      optionText : function() || "string" || undefined, 
     *      onChange : function() 
     * } 
     */
    mounted: function () {
        var options = this.model.options;
        // set default selected value to the first element of options list
        if (options.constructor.name === 'Array' && options[0] != undefined && this.model.selected === undefined) {
            this.model.selected = options[0];
        }
    },
    methods: {
        getText: function (option) {
            var optionText = this.model.optionText;
            if (typeof optionText == "function") {
                // assuming function will retrun option text
                return optionText(option);
            } else if (typeof optionText == "string") {
                // assumming optionText contains name of the option object property that should be displayed as option text
                return option[optionText];
            } else if (option.text != undefined) {
                return option.text;
            } else if (optionText === undefined) {
                return option;
            }
            // otherwise return first property of option object
            return option[Object.keys(option)[0]];
        }
    },
    watch: {
        'model.selected': function (newVal, oldVal) {
            var options = this.model.options;
            // check if setting value that corresponds to options
            var match = false;
            if (XT.ObjectModel.same(newVal, oldVal)) {
                return;
            }
            if (newVal != undefined) {
                for (i = 0; i < options.length; i++) {
                    if (JSON.stringify(newVal) == JSON.stringify(options[i])) {
                        // match found 
                        match = true;
                        // set selection exacly to one of the options inside list
                        if (newVal !== options[i]) {
                            this.model.selected = options[i]
                        }
                        break;
                    }
                }
                if (!match) {
                    if (oldVal === undefined) {
                        this.model.selected = oldVal;
                    }
                    throw '.selected value ' + JSON.stringify(newVal) + '  does not match any of the options';
                }
            }

            // when selected value changes raise event
            if (typeof this.model.onChange == 'function') {
                this.model.onChange(newVal, oldVal);
            }
        },
        'model.options': function (newOptions, oldOptions) {
            if (newOptions !== oldOptions && newOptions[0] != undefined) {
                // when new options are set 
                // set default selected value to the first element of options list
                this.model.selected = newOptions[0];
            }
        }
    }
});

