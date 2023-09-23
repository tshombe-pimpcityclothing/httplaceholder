#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
. $DIR/setup.sh
curl --location --request POST "$HTTPL_ROOT_URL/dynamic-form-post.txt" \
--header "Content-Type: application/x-www-form-urlencoded" \
--data-raw "formval1=value 1&formval2=value 2' -D-