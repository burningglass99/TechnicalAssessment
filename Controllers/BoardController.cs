using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TechnicalAssessment.Models;

namespace TechnicalAssessment.Controllers
{
    /// <summary>
    /// Controller for the interactions with the board
    /// board is the active board
    /// nodeFlag is flag determining whether there is an active node or not ie the first node has already been clicked
    /// playerTwoFlag is used to determine which players turn it is, true = player 2's turn
    /// activeNode is the valid start node
    /// </summary>
    public class BoardController : Controller
    {
        private Board board;
        private bool nodeFlag;
        private bool playerTwoFlag;
        private Point activeNode;

        /// <summary>
        /// Constructor for the BoardController Class
        /// </summary>
        public BoardController()
        {
            board = new Board();
            activeNode = new Point();
        }
        
        /// <summary>
        /// Class to interpret the json sent from the client, digest it, and determine what to send back
        /// </summary>
        /// <param name="jsonRequest">The json from the client</param>
        /// <var name="jsonResponse">The generated json response string if there is one, initialized to empty string</var>
        /// <var name="response">The object representing the json response</var>
        /// <returns>Generated json response string</returns>
        public string RequestInterpretor(string jsonRequest)
        {
            string jsonResponse = "";
            ResponsePayload response = new ResponsePayload();
            response.body = new StateUpdate();
            if (jsonRequest.Length > 0)
            {
                //Since body could be an object or a string depending on what the msg field is parse
                //the json to find out if msg is "NODE_CLICKED"
                var parsedJsonRequest = JObject.Parse(jsonRequest);
                var requestMessage = (string)parsedJsonRequest["msg"];
                if (string.Equals(requestMessage, "NODE_CLICKED"))
                {
                    PointPayload pPayload = JsonConvert.DeserializeObject<PointPayload>(jsonRequest);
                    Point point = new Point();
                    point.x = pPayload.body.x;
                    point.y = pPayload.body.y;
                    response.id = pPayload.id;

                    //If nodeFlag is true, this node click represents the second attempted click to complete a line
                    if (nodeFlag)
                    {
                        bool validNode = isValidNode(point);
                        if (validNode)
                        {
                            Line newLine = new Line();
                            newLine.start = new Point();
                            newLine.end = new Point();

                            board.nodes[activeNode.x, activeNode.y].Activated = true;

                            //If this is first valid move, only activate start node and don't close it
                            if (!board.GameStarted)
                            {
                                board.GameStarted = true;
                            }
                            else
                            {
                                board.nodes[activeNode.x, activeNode.y].Closed = true;
                            }
                            board.nodes[point.x, point.y].Activated = true;

                            //Activate and close nodes directly between start and end nodes
                            activateMiddleNodes(point);
                            
                            if (isGameOver())
                            {
                                response.msg = "GAME_OVER";
                                response.body.heading = "Game Over";
                                if (playerTwoFlag)
                                {
                                    response.body.message = "Player 1 Wins!";
                                }
                                else
                                {
                                    response.body.message = "Player 2 Wins!";
                                }
                            }
                            else
                            {
                                response.msg = "VALID_END_NODE";
                                if (playerTwoFlag)
                                {
                                    response.body.heading = "Player 1";
                                    playerTwoFlag = false;
                                }
                                else
                                {
                                    response.body.heading = "Player 2";
                                    playerTwoFlag = true;
                                }
                                response.body.message = null;
                            }
                            newLine.start.x = activeNode.x;
                            newLine.start.y = activeNode.y;
                            newLine.end.x = point.x;
                            newLine.end.y = point.y;
                            response.body.newLine = newLine;
                            jsonResponse = JsonConvert.SerializeObject(response);
                        }
                        else
                        {
                            response.msg = "INVALID_END_NODE";
                            response.body.newLine = null;
                            if (playerTwoFlag)
                            {
                                response.body.heading = "Player 2";
                            }
                            else
                            {
                                response.body.heading = "Player 1";
                            }
                            response.body.message = "Invalid move!";
                            jsonResponse = JsonConvert.SerializeObject(response);
                        }
                        nodeFlag = false;
                    }
                    else
                    {
                        //If first move, any node is viable so less checks need to be performed
                        if (board.GameStarted)
                        {
                            if (board.nodes[point.x, point.y].Activated && !board.nodes[point.x, point.y].Closed)
                            {
                                nodeFlag = true;
                                activeNode.x = point.x;
                                activeNode.y = point.y;
                                response.body.newLine = null;
                                response.body.heading = "Player 1";
                                response.body.message = "Select a second node to complete the line.";
                                response.msg = "VALID_START_NODE";
                                jsonResponse = JsonConvert.SerializeObject(response);
                            }
                            else
                            {
                                response.body.newLine = null;
                                if (playerTwoFlag)
                                {
                                    response.body.heading = "Player 2";
                                }
                                else
                                {
                                    response.body.heading = "Player 1";
                                }
                                response.body.message = "Not a valid starting position";
                                response.msg = "INVALID_START_NODE";
                                response.body = response.body;
                                jsonResponse = JsonConvert.SerializeObject(response);
                            }
                        }
                        else
                        {
                            nodeFlag = true;
                            activeNode.x = point.x;
                            activeNode.y = point.y;
                            response.body.newLine = null;
                            response.body.heading = "Player 1";
                            response.body.message = "Select a second node to complete the line.";
                            response.msg = "VALID_START_NODE";
                            jsonResponse = JsonConvert.SerializeObject(response);
                        }
                    }
                }
                else
                {
                    //Either reinitialize the board or don't respond with anything as there was an error
                    Payload payload = JsonConvert.DeserializeObject<Payload>(jsonRequest);
                    if (string.Equals(payload.msg, "INITIALIZE"))
                    {
                        board = new Board();
                        activeNode = new Point();
                        nodeFlag = false;
                        response.id = payload.id;
                        response.msg = payload.msg;
                        response.body.newLine = null;
                        response.body.heading = "Player 1";
                        response.body.message = "Awaiting Player 1's Move";
                        jsonResponse = JsonConvert.SerializeObject(response);
                    }
                    else
                    {
                        jsonResponse = "";
                    }
                }
            }
            return jsonResponse;
        }

        /// <summary>
        /// Checks to see if second node clicked is valid along with nodes inbetween if the nodes are
        /// more than 1 node apart
        /// </summary>
        /// <param name="point">Node clicked as end node</param>
        /// <returns>True if point is a valid move, False if not</returns>
        private bool isValidNode(Point point)
        {
            int spaceDifference, xDiff, yDiff, newX, newY;
            string direction;
            xDiff = Math.Abs(activeNode.x - point.x);
            yDiff = Math.Abs(activeNode.y - point.y);
            if (yDiff > xDiff && xDiff == 0)
            {
                spaceDifference = yDiff;
                direction = "vertical";
            }
            else if (xDiff > yDiff && yDiff == 0)
            {
                spaceDifference = xDiff;
                direction = "horizontal";
            }
            else if (xDiff == yDiff && xDiff != 0)
            {
                spaceDifference = xDiff;
                direction = "diagonal";
            }
            else
            {
                return false;
            }
            if(spaceDifference == 1)
            {
                if (!board.nodes[point.x, point.y].Activated)
                {
                    return true;
                }
            }
            else
            {
                for (int i = 1; i < spaceDifference; i++)
                {
                    if (string.Equals(direction, "vertical"))
                    {
                        newY = (activeNode.y + point.y) * i / spaceDifference;
                        if (board.nodes[point.x, newY].Activated)
                        {
                            return false;
                        }
                    }
                    else if (string.Equals(direction, "horizontal"))
                    {
                        newX = (activeNode.x + point.x) * i / spaceDifference;
                        if (board.nodes[newX, point.y].Activated)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        newX = (activeNode.x + point.x) * i / spaceDifference;
                        newY = (activeNode.y + point.y) * i / spaceDifference;
                        if (board.nodes[newX, newY].Activated)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Method to determine which nodes that weren't explicitly selected to activate and close
        /// </summary>
        /// <param name="point">Point that the new line will end on</param>
        private void activateMiddleNodes(Point point)
        {
            int spaceDifference, xDiff, yDiff;
            string direction;
            xDiff = Math.Abs(activeNode.x - point.x);
            yDiff = Math.Abs(activeNode.y - point.y);
            Point midPoint = new Point();
            if (yDiff > xDiff)
            {
                spaceDifference = yDiff;
                direction = "vertical";
            }
            else if (xDiff > yDiff)
            {
                spaceDifference = xDiff;
                direction = "horizontal";
            }
            else
            {
                spaceDifference = xDiff;
                direction = "diagonal";
            }
            if (spaceDifference > 1)
            {
                for (int i = 1; i < spaceDifference; i++)
                {
                    if (string.Equals(direction, "vertical"))
                    {
                        midPoint.x = point.x;
                        midPoint.y = (activeNode.y + point.y) * i / spaceDifference;
                        closeNode(midPoint);
                    }
                    else if (string.Equals(direction, "horizontal"))
                    {
                        midPoint.x = (activeNode.x + point.x) * i / spaceDifference;
                        midPoint.y = point.y;
                        closeNode(midPoint);
                    }
                    else
                    {
                        midPoint.x = (activeNode.x + point.x) * i / spaceDifference;
                        midPoint.y = (activeNode.y + point.y) * i / spaceDifference;
                        closeNode(midPoint);
                    }
                }
            }
        }

        /// <summary>
        /// Method to activate and close a particular node
        /// </summary>
        /// <param name="point">Point to be activated and closed</param>
        private void closeNode(Point point)
        {
            board.nodes[point.x, point.y].Activated = true;
            board.nodes[point.x, point.y].Closed = true;
        }

        /// <summary>
        /// Method to check whether there are any valid moves left
        /// </summary>
        /// <returns>True if there are no valid moves, False if at least one move remains</returns>
        private bool isGameOver()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    //If node on board is found to be activated and not closed than it is an end node
                    //so we proceed to check if any nodes around it aren't activated as long as they exist
                    if (board.nodes[i,j].Activated && !board.nodes[i, j].Closed)
                    {
                        if (i > 0)
                        {
                            if (!board.nodes[i - 1, j].Activated)
                            {
                                return false;
                            }
                            if (j > 0)
                            {
                                if (!board.nodes[i - 1, j - 1].Activated)
                                {
                                    return false;
                                }
                            }
                            if (j < 3)
                            {
                                if (!board.nodes[i - 1, j + 1].Activated)
                                {
                                    return false;
                                }
                            }
                        }
                        if (i < 3)
                        {
                            if (!board.nodes[i + 1, j].Activated)
                            {
                                return false;
                            }
                            if (j > 0)
                            {
                                if (!board.nodes[i + 1, j - 1].Activated)
                                {
                                    return false;
                                }
                            }
                            if (j < 3)
                            {
                                if (!board.nodes[i + 1, j + 1].Activated)
                                {
                                    return false;
                                }
                            }
                        }
                        if (j > 0)
                        {
                            if (!board.nodes[i, j - 1].Activated)
                            {
                                return false;
                            }
                        }
                        if (j < 3)
                        {
                            if (!board.nodes[i, j + 1].Activated)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
