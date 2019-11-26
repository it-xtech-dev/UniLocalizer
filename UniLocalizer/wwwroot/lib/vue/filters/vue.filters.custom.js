/* Vue custom filters
 * Requires https://momentjs.com/
 * Copyright: it.xtech.pl
 */

// standard date formatter;
Vue.filter('formatDate', function (value, format) {
    if (value) {
        var _format = format || 'YYYY-MM-DD';
        return moment(String(value)).format(_format);
    }
});