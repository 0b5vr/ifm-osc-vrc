module.exports = function( x, x0, x1, y0, y1 ) {
  return ( ( x - x0 ) * ( y1 - y0 ) / ( x1 - x0 ) + y0 );
}
