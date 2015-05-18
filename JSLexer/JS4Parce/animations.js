jQuery(document).ready(function($) {
(function() {
                        function getRandomInt (min, max) {
                            return Math.floor(Math.random() * (max - min + 1)) + min;
                        }
 
                        var number = 10;
                        var range = [
                                {x:-410,y:500},
                                {x:310,y:-150}
                        ];
 
                        function create() {
                                var img = $("<img>");
                                img.attr("src", "../images/animCircles/circle" + getRandomInt(1,5) + ".png");
                                img.attr("width", getRandomInt(60, 120));
 
                                img.css("position", "absolute");
                                img.css("margin-left", getRandomInt(range[0].x, range[1].x));
                                //img.css("margin-right", getRandomInt(range[0].x, range[1].x));
                                img.css("top", 360);
                                img.css("z-index", -1);
 
                                img.animate({top: range[1].y}, getRandomInt(3000, 9000), function() {
                                        $(this).remove();
                                        create();
                                });
 
                                img.insertBefore($("#logo"));
                        }
 
                        for(var i = 0; i < number; i++) {
                                create();
                        }
 })();
});