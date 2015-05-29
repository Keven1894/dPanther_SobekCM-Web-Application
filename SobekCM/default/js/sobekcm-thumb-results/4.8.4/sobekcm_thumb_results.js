
//Code using JQuery qTip
$(document).ready(function() {
  //  alert('calling this function successfully');
    $('[id*=sbkThumbnailImg]').each(function () {

        var $this = $(this);
        var title_count = $this.attr('id').split('g')[1];
        var divContent = $('#descThumbnail' + title_count).html();
     //   var titleText = '<div style="color:red;"><b>' + $this.parent().parent().parent().parent().next().find('span').html() + '</b></div>';
        var myPosition = 'center left';
        var atPosition = 'center right';
        //Set the location based on the thumbnail column number. If the image is in the fourth and last column, the tooltip should display on the left
        if (title_count % 4 == 0) {
            myPosition = 'center right';
            atPosition = 'center left';
        }


         $this.qtip(
            {

                content: {
                      text:  divContent
                },
                position: {
                    my: myPosition,
                    at: atPosition
                },
                style: {
                    width: 500,
                     border: 1,
                    
                    hide: {
                        fixed: true,
                        delay:1000
                    },
                    classes: 'qtip-tipsy',
                    tip: {
                        width: 20
                    }
                }
    
            });   
              
            });
    });