<?php 
        $db = mysql_connect('games.adnsystems.com', 'fhiadmin', 'T3nt$rcold') or die('Could not connect: ' . mysql_error()); 
        mysql_select_db('fhipuzzledb') or die('Could not select database');
 
        // Strings must be escaped to prevent SQL injection attack. 
        $name = mysql_real_escape_string($_GET['name'], $db); 
        $score = mysql_real_escape_string($_GET['score'], $db); 
        $hash = $_GET['hash']; 
 
        $secretKey="hakunamatata"; # Change this value to match the value stored in the client javascript below 

        $real_hash = md5($name . $score . $secretKey); 
        $hash=1;
        $real_hash=1;
        if($real_hash == $hash) { 
            // Send variables for the MySQL database class. 
            $query = "insert into scores values (NULL, '$name', '$score','origin');"; 
            $result = mysql_query($query) or die('Query failed: ' . mysql_error()); 
        } 
?>